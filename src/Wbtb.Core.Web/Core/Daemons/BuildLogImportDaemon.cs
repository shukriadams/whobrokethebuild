using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Web.Core.Daemons
{
    /// <summary>
    /// Runs import build and import log on build systems.
    /// </summary>
    public class BuildLogImportDaemon : IWebDaemon
    {
        #region FIELDS

        private ILogger<BuildLogImportDaemon> _log;

        private IDaemonProcessRunner _processRunner;

        #endregion

        #region CTORS

        public BuildLogImportDaemon(ILogger<BuildLogImportDaemon> log, IDaemonProcessRunner processRunner)
        {
            _log = log;
            _processRunner = processRunner;
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
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();

            // start daemons - this should be folded into start
            foreach (BuildServer cfgbuildServer in ConfigKeeper.Instance.BuildServers)
            {
                BuildServer buildServer = dataLayer.GetBuildServerByKey(cfgbuildServer.Key);
                // note : buildserver can be null if trying to run daemon before auto data injection has had time to run
                if (buildServer == null)
                    continue;

                IBuildServerPlugin buildServerPlugin = PluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
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
                        if (!job.Enable)
                            continue;

                        Job thisjob = dataLayer.GetJobByKey(job.Key);
                        if (thisjob.ImportCount.HasValue)
                            count = thisjob.ImportCount.Value;

                        IEnumerable<Build> processedBuilds = buildServerPlugin.ImportLogs(thisjob);
                        _log.LogInformation($"Imported {processedBuilds.Count()} logs.");

                        foreach (Build build in processedBuilds)
                            BuildLevelPluginHelper.InvokeEvents("OnLogAvailable", job.OnLogAvailable, build);
                    }
                    catch (Exception ex)
                    {
                        _log.LogError($"Unexpected error trying to import jobs/logs for \"{job.Key}\" from buildserver \"{buildServer.Key}\" : {ex}");
                    }
                }
            }
        }

        #endregion
    }
}
