using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;

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

                        // handle current state of game
                        Build latestBuild = dataLayer.GetLatestBuildByJob(thisjob);
                        Build previousDeltaBuild = dataLayer.GetLastJobDelta(thisjob.Id);

                        // no builds for this job yet
                        if (latestBuild == null)
                            continue;

                        // ignore builds that don't have incidents yet, they need processing by the incident assign daemon first
                        if (latestBuild.Status == BuildStatus.Failed && latestBuild.IncidentBuildId == null)
                            continue;

                        Build buildtoInformOn = previousDeltaBuild;
                        if (buildtoInformOn == null)
                            buildtoInformOn = latestBuild;

                        // this build is first, so it is the first delta
                        if (previousDeltaBuild == null)
                        {
                            dataLayer.SaveJobDelta(latestBuild);
                        }
                        else
                        {
                            if (latestBuild.Status == BuildStatus.Failed && previousDeltaBuild.Status == BuildStatus.Passed)
                            {
                                // build has gone from passing to failing
                                dataLayer.SaveJobDelta(latestBuild);
                            }
                            else if (latestBuild.Status == BuildStatus.Passed && previousDeltaBuild.Status == BuildStatus.Failed)
                            {
                                // build has gone from failing to passing
                                dataLayer.SaveJobDelta(latestBuild);
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
