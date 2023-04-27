using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Wbtb.Core.Common;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Runs import build and import log on build systems.
    /// </summary>
    public class BuildImportDaemon : IWebDaemon
    {
        #region FIELDS

        private ILogger<BuildImportDaemon> _log;

        private IDaemonProcessRunner _processRunner;

        #endregion

        #region CTORS

        public BuildImportDaemon(ILogger<BuildImportDaemon> log, IDaemonProcessRunner processRunner)
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

                        BuildImportSummary importSummary = buildServerPlugin.ImportBuilds(thisjob, count);

                        foreach (Build build in importSummary.Ended)
                        {
                            // set delta
                            Build previousBuild = dataLayer.GetPreviousBuild(build);
                            BuildDelta? delta = null;
                            if (previousBuild == null)
                            { 
                                // this build is first build
                                if (build.Status == BuildStatus.Failed)
                                    delta = BuildDelta.Broke;
                                else if (build.Status == BuildStatus.Passed)
                                    delta = BuildDelta.Pass;
                                else if (build.Status == BuildStatus.InProgress || build.Status == BuildStatus.Queued || build.Status == BuildStatus.Unknown)
                                { 
                                    // this should not happen on build end, import has failed somehow
                                    delta = BuildDelta.Unknown;
                                    _log.LogError($"Invalid status detected on completed build {build.Id}");
                                }
                            }

                            if (delta != null)
                            { 
                                build.Delta = delta.Value;
                                dataLayer.SaveBuild(build);
                            }
                        }

                        // fire events
                        foreach (Build build in importSummary.Created)
                            BuildLevelPluginHelper.InvokeEvents("OnBuildStart", job.OnBuildStart, build);

                        foreach (Build build in importSummary.Ended)
                            BuildLevelPluginHelper.InvokeEvents("OnBuildEnd", job.OnBuildEnd, build);

                        // handle current state of game
                        Build latestBuild = importSummary.Ended.OrderByDescending(b => b.EndedUtc.Value).FirstOrDefault();
                        if (latestBuild != null)
                        { 
                            Build lastDeltaBuild = dataLayer.GetLastJobDelta(thisjob.Id);
                            if (latestBuild.Status == BuildStatus.Failed && latestBuild.Status == BuildStatus.Passed)
                            {
                                // build has gone from failing to passing
                                BuildLevelPluginHelper.InvokeEvents("OnBroken", job.OnBroken, latestBuild);
                                dataLayer.SaveJobDelta(latestBuild);
                            }
                            else if(latestBuild.Status == BuildStatus.Passed && latestBuild.Status == BuildStatus.Failed)
                            {
                                // build has gone from failing to passing
                                BuildLevelPluginHelper.InvokeEvents("OnFixed", job.OnFixed, latestBuild);
                                dataLayer.SaveJobDelta(latestBuild);
                            }
                        }
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
