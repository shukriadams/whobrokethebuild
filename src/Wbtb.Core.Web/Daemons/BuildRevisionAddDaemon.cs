using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;
using Wbtb.Core.Web.Daemons;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Adds revisions in build, assuming job supports revision-in-build lookup at build server. If not, must read revisions in build from 
    /// log, which has its own daemon.
    /// </summary>
    public class BuildRevisionAddDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly ILogger _log;

        private readonly IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        private readonly BuildLevelPluginHelper _buildLevelPluginHelper;

        private readonly SimpleDI _di;

        public static int TaskGroup = 0;

        #endregion

        #region CTORS

        public BuildRevisionAddDaemon(ILogger log, IDaemonProcessRunner processRunner)
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
            IEnumerable<DaemonTask> tasks = dataLayer.GetPendingDaemonTasksByTask(DaemonTaskTypes.AddBuildRevisionsFromBuildServer.ToString());

            foreach(DaemonTask task in tasks)
            {
                Build build = dataLayer.GetBuildById(task.BuildId);
                Job job = dataLayer.GetJobById(build.JobId);
                BuildServer buildServer = dataLayer.GetBuildServerByKey(job.BuildServer);
                IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
                ReachAttemptResult reach = buildServerPlugin.AttemptReach(buildServer);

                if (!reach.Reachable)
                {
                    _log.LogError($"Buildserver {buildServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}");
                    continue;
                }

                // no need check if blocked

                IEnumerable<string> revisionCodes = buildServerPlugin.GetRevisionsInBuild(build);
                foreach (string revisionCode in revisionCodes)
                {
                    try
                    {
                        string biID = dataLayer.SaveBuildInvolement(new BuildInvolvement
                        {
                            BuildId = build.Id,
                            RevisionCode = revisionCode
                        }).Id;

                        task.ProcessedUtc = DateTime.UtcNow;
                        task.HasPassed = true;
                        dataLayer.SaveDaemonTask(task);

                        dataLayer.SaveDaemonTask(new DaemonTask { 
                            TaskKey = DaemonTaskTypes.RevisionResolve.ToString(),
                            BuildId = build.Id,
                            BuildInvolvementId = biID,
                            Src = this.GetType().Name,
                            Order = 2,
                        });

                        dataLayer.SaveDaemonTask(new DaemonTask
                        {
                            TaskKey = DaemonTaskTypes.UserResolve.ToString(),
                            BuildId = build.Id,
                            BuildInvolvementId = biID,
                            Src = this.GetType().Name,
                            Order = 2,
                        });
                    }
                    catch (Exception ex)
                    {
                        task.ProcessedUtc = DateTime.UtcNow;
                        task.HasPassed = false;
                        task.Result = ex.ToString();
                        dataLayer.SaveDaemonTask(task);
                    }
                }

            }

        }

        #endregion
    }
}
