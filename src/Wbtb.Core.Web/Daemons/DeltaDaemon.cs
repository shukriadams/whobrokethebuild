using Microsoft.Extensions.Logging;
using System;
using Wbtb.Core.Common;
using YamlDotNet.Serialization.NodeTypeResolvers;

namespace Wbtb.Core.Web
{
    public class DeltaDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly ILogger _log;

        private readonly IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly Config _config;

        private readonly BuildLevelPluginHelper _buildLevelPluginHelper;

        private readonly SimpleDI _di;
        #endregion

        #region CTORS

        public DeltaDaemon(ILogger log, IDaemonProcessRunner processRunner)
        {
            _log = log;
            _processRunner = processRunner;

            _di = new SimpleDI();
            _config = _di.Resolve<Config>();
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
            IDataLayerPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataLayerPlugin>();

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
                        Build latestBuild = dataLayer.GetLatestBuildByJob(thisjob);
                        Build previousDeltaBuild = dataLayer.GetLastJobDelta(thisjob.Id);
                        bool alertFailing = false;
                        bool alertPassing = false;

                        // build delta is correct, ignore
                        if (latestBuild != null && previousDeltaBuild != null && latestBuild.Status == previousDeltaBuild.Status) 
                            continue;

                        Build deltaLookup = dataLayer.GetDeltaBuildAtBuild(latestBuild);

                        if (previousDeltaBuild == null)
                        {
                            // this build is first, so it is the first delta
                            dataLayer.SaveJobDelta(latestBuild);
                            if (latestBuild.Status == BuildStatus.Failed)
                                alertFailing = true;
                        }
                        else
                        {
                            if (latestBuild.Status == BuildStatus.Failed && previousDeltaBuild.Status == BuildStatus.Passed)
                            {
                                // build has gone from passing to failing
                                dataLayer.SaveJobDelta(latestBuild);
                                _buildLevelPluginHelper.InvokeEvents("OnBroken", job.OnBroken, latestBuild);
                                alertFailing = true;
                            }
                            else if (latestBuild.Status == BuildStatus.Passed && previousDeltaBuild.Status == BuildStatus.Failed)
                            {
                                // build has gone from failing to passing
                                dataLayer.SaveJobDelta(latestBuild);
                                _buildLevelPluginHelper.InvokeEvents("OnFixed", job.OnFixed, latestBuild);
                                alertPassing = true;
                            }
                        }

                        if (alertFailing)
                            foreach (MessageHandler alert in job.Message)
                            {
                                IMessaging messagePlugin = _pluginProvider.GetByKey(alert.Plugin) as IMessaging;
                                messagePlugin.AlertBreaking(alert, latestBuild);
                            }

                        if (alertPassing)
                        {
                            Build incidentCausingBuild = dataLayer.GetBuildById(previousDeltaBuild.IncidentBuildId);

                            foreach (MessageHandler alert in job.Message)
                            {
                                IMessaging messagePlugin = _pluginProvider.GetByKey(alert.Plugin) as IMessaging;
                                messagePlugin.AlertPassing(alert, incidentCausingBuild, latestBuild);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        _log.LogError($"Unexpected error trying to import builds for \"{job.Key}\" from buildserver \"{buildServer.Key}\" : {ex}");
                    }
                } //foreach job
            } // foreach buildserver
        }

        #endregion
    }
}
