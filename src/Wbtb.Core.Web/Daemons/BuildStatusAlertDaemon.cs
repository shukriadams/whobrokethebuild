using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Runs import build and import log on build systems.
    /// </summary>
    public class BuildStatusAlertDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly ILogger _log;

        private readonly IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        private readonly BuildLevelPluginHelper _buildLevelPluginHelper;

        private readonly SimpleDI _di;

        #endregion

        #region CTORS

        public BuildStatusAlertDaemon(ILogger log, IDaemonProcessRunner processRunner)
        {
            _log = log;
            _processRunner = processRunner;

            _di = new SimpleDI();
            _config = _di.Resolve<Configuration>();
            _pluginProvider = _di.Resolve<PluginProvider>();
            _buildLevelPluginHelper = _di.Resolve<BuildLevelPluginHelper>();
        }

        #endregion

        #region METHODS

        public void Start(int tickInterval)
        {
            _processRunner.Start(new DaemonWork(this.Work), tickInterval);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _processRunner.Dispose();
        }

        /// <summary>
        /// Daemon's main work method
        /// </summary>
        private void Work()
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();

            // start daemons - this should be folded into start
            foreach (BuildServer cfgbuildServer in _config.BuildServers)
            {
                BuildServer buildServer = dataLayer.GetBuildServerByKey(cfgbuildServer.Key);
                // note : buildserver can be null if trying to run daemon before auto data injection has had time to run
                if (buildServer == null)
                    continue;

                IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
                ReachAttemptResult reach = buildServerPlugin.AttemptReach(buildServer);

                int count = 100;
                if (buildServer.ImportCount.HasValue)
                    count = buildServer.ImportCount.Value;

                if (!reach.Reachable)
                {
                    _log.LogError($"Buildserver {buildServer.Key} not reachable, job import aborted {reach.Error}{reach.Exception}");
                    return;
                }

                foreach (Job job in buildServer.Jobs)
                {
                    try
                    {
                        Job thisjob = dataLayer.GetJobByKey(job.Key);

                        // handle current state of game
                        Build deltaBuild = dataLayer.GetLastJobDelta(thisjob.Id);

                        // if delta not yet calculated, ignore alerts for this job
                        if (deltaBuild == null)
                            continue;

                        // check if delta has already been alerted on
                        string deltaAlertKey = $"delta_alert_{deltaBuild.Id}";
                        StoreItem deltaAlerted = dataLayer.GetStoreItemByKey(deltaAlertKey);
                        if (deltaAlerted != null)
                            continue;

                        // check if log processing errors occurred - if not, ensure that all log processors have run. Else, alert on error, alert plugin
                        // should pick up on error and generate appropriate message
                        IEnumerable<BuildFlag> flags = dataLayer.GetBuildFlagsForBuild(deltaBuild);
                        if (deltaBuild.Status == BuildStatus.Failed && !flags.Where(f => f.Flag == BuildFlags.LogParseFailed).Any()) 
                        {
                            // 
                            bool allLogsParsed = true;
                            IEnumerable<BuildLogParseResult> parseResults = dataLayer.GetBuildLogParseResultsByBuildId(deltaBuild.Id);
                            foreach (string parser in job.LogParserPlugins)
                                if (!parseResults.Where(r => r.LogParserPlugin == parser).Any())
                                {
                                    allLogsParsed = false;
                                    break;
                                }

                            // wait for logs to finish processing
                            if (!allLogsParsed)
                                continue;
                        }

                        if (deltaBuild.Status == BuildStatus.Failed)
                        {
                            // build has gone from passing to failing
                            _buildLevelPluginHelper.InvokeEvents("OnBroken", job.OnBroken, deltaBuild);
                            foreach (MessageHandler alert in job.Message)
                            {
                                IMessagingPlugin messagePlugin = _pluginProvider.GetByKey(alert.Plugin) as IMessagingPlugin;
                                messagePlugin.AlertBreaking(alert, deltaBuild);
                            }

                        }
                        else if (deltaBuild.Status == BuildStatus.Passed)
                        {
                            // build has gone from failing to passing
                            _buildLevelPluginHelper.InvokeEvents("OnFixed", job.OnFixed, deltaBuild);
                            string lastbreakingId = dataLayer.GetIncidentIdsForJob(thisjob).FirstOrDefault();
                            Build lastBreakingBuild = null;

                            if (!string.IsNullOrEmpty(lastbreakingId)) 
                                lastBreakingBuild = dataLayer.GetBuildById(lastbreakingId);

                            // ugly cludge 
                            if (lastBreakingBuild == null)
                                lastBreakingBuild = deltaBuild;

                            foreach (MessageHandler alert in job.Message)
                            {
                                IMessagingPlugin messagePlugin = _pluginProvider.GetByKey(alert.Plugin) as IMessagingPlugin;
                                messagePlugin.AlertPassing(alert, lastBreakingBuild, deltaBuild);
                            }
                        }

                        dataLayer.SaveStore(new StoreItem { 
                            Key = deltaAlertKey,
                            Plugin = this.GetType().Name
                        });

                    }
                    catch (Exception ex)
                    {
                        _log.LogError($"Unexpected error trying to import jobs/logs for \"{job.Key}\" from buildserver \"{buildServer.Key}\" : {ex}");
                    }
                } //foreach job
            } // foreach buildserver
        }

        #endregion
    }
}
