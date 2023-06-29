using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;
using Wbtb.Core.Web.Daemons;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Runs import build and import log on build systems.
    /// </summary>
    public class BuildStartDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly ILogger _log;

        private readonly IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        private readonly BuildEventHandlerHelper _buildLevelPluginHelper;

        private readonly SimpleDI _di;
        #endregion

        #region CTORS

        public BuildStartDaemon(ILogger log, IDaemonProcessRunner processRunner)
        {
            _log = log;
            _processRunner = processRunner;

            _di = new SimpleDI();
            _config = _di.Resolve<Configuration>();
            _pluginProvider = _di.Resolve<PluginProvider>();
            _buildLevelPluginHelper = _di.Resolve<BuildEventHandlerHelper>();
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

            foreach (BuildServer cfgbuildServer in _config.BuildServers)
            {
                BuildServer buildServer = dataLayer.GetBuildServerByKey(cfgbuildServer.Key);
                IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
                ReachAttemptResult reach = buildServerPlugin.AttemptReach(buildServer);

                if (!reach.Reachable)
                {
                    _log.LogError($"Buildserver {buildServer.Key} not reachable, job import aborted {reach.Error}{reach.Exception}");
                    continue;
                }

                foreach (Job job in buildServer.Jobs)
                {
                    try
                    {
                        Job jobInDB = dataLayer.GetJobByKey(job.Key);
                        buildServerPlugin.PollBuildsForJob(jobInDB);
                        IEnumerable<Build> latestBuilds = buildServerPlugin.GetLatesBuilds(jobInDB, job.ImportCount);
                        // get latest page of build for quick lookup
                        IEnumerable<Build> existingBuilds = dataLayer.PageBuildsByJob(jobInDB.Id, 0, job.ImportCount * 2, false).Items;

                        foreach (Build latestBuild in latestBuilds) 
                        {
                            // check if incoming build is in latest page, this will happen most frequently, and is a cheap check
                            if (existingBuilds.FirstOrDefault(b => b.Identifier == latestBuild.Identifier) != null)
                                continue;

                            // make certain build doesnt't exist in db
                            if (dataLayer.GetBuildByKey(jobInDB.Id, latestBuild.Identifier) != null)
                                continue;

                            latestBuild.JobId = jobInDB.Id;
                            string buildId = dataLayer.SaveBuild(latestBuild).Id;

                            _buildLevelPluginHelper.InvokeEvents("OnBuildStart", job.OnBuildStart, latestBuild);

                            // create next task in chain
                            dataLayer.SaveDaemonTask(new DaemonTask{
                                TaskKey = DaemonTaskTypes.BuildEnd.ToString(),
                                Order = 0,
                                Src = this.GetType().Name,
                                BuildId = buildId
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.LogError($"Unexpected error trying to import builds for \"{job.Key}\" from buildserver \"{buildServer.Key}\" : {ex}");
                    }
                }
            }
        }

        #endregion
    }
}
