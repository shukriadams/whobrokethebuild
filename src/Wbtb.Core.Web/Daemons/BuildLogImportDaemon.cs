using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;
using Wbtb.Core.Web.Daemons;

namespace Wbtb.Core.Web.Core
{
    /// <summary>
    /// Runs import build and import log on build systems.
    /// </summary>
    public class BuildLogImportDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly ILogger _log;

        private readonly IDaemonProcessRunner _processRunner;
        
        private readonly PluginProvider _pluginProvider;

        private readonly SimpleDI _di;

        public static int TaskGroup = 1;

        #endregion

        #region CTORS

        public BuildLogImportDaemon(ILogger log, IDaemonProcessRunner processRunner)
        {
            _log = log;
            _processRunner = processRunner;
            _di = new SimpleDI();
            _pluginProvider = _di.Resolve<PluginProvider>();
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
            IEnumerable<DaemonTask> tasks = dataLayer.GetPendingDaemonTasksByTask(DaemonTaskTypes.LogImport.ToString());
            TaskDaemonProcesses daemonProcesses = _di.Resolve<TaskDaemonProcesses>();

            try
            {
                foreach (DaemonTask task in tasks)
                {
                    try
                    {
                        Build build = dataLayer.GetBuildById(task.BuildId);
                        Job job = dataLayer.GetJobById(build.JobId);
                        BuildServer buildServer = dataLayer.GetBuildServerByKey(job.BuildServer);
                        IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
                        ReachAttemptResult reach = buildServerPlugin.AttemptReach(buildServer);

                        daemonProcesses.AddActive(this, $"Task : {task.Id}, Build {build.Id}");

                        if (!reach.Reachable)
                        {
                            _log.LogError($"Buildserver {buildServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}");
                            daemonProcesses.TaskBlocked(task, $"Buildserver {buildServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}");
                            continue;
                        }

                        IEnumerable<DaemonTask> blocking = dataLayer.DaemonTasksBlocked(build.Id, TaskGroup);
                        if (blocking.Any()) 
                        {
                            daemonProcesses.TaskBlocked(task, this, blocking);
                            continue;
                        }

                        BuildLogRetrieveResult result = buildServerPlugin.ImportLog(build);
                        task.Result = result.Result;
                        task.ProcessedUtc = DateTime.UtcNow;

                        if (!result.Success)
                        {
                            task.HasPassed = false;
                            dataLayer.SaveDaemonTask(task);
                            daemonProcesses.TaskDone(task);
                            continue;
                        }

                        build.LogPath = result.BuildLogPath;
                        dataLayer.SaveBuild(build);

                        task.HasPassed = true;
                        dataLayer.SaveDaemonTask(task);
                        daemonProcesses.TaskDone(task);

                        // create tasks for next stage
                        foreach (string logparser in job.LogParsers)
                            dataLayer.SaveDaemonTask(new DaemonTask
                            {
                                BuildId = build.Id,
                                Src = this.GetType().Name,
                                Args = logparser,
                                Order = 3,
                                TaskKey = DaemonTaskTypes.LogParse.ToString()
                            });

                        // build revision requires source controld
                        if (!string.IsNullOrEmpty(job.RevisionAtBuildRegex) && !string.IsNullOrEmpty(job.SourceServerId))
                            dataLayer.SaveDaemonTask(new DaemonTask
                            {
                                BuildId = build.Id,
                                Src = this.GetType().Name,
                                Order = 2,
                                TaskKey = DaemonTaskTypes.AddBuildRevisionsFromBuildLog.ToString()
                            });
                    }
                    catch (Exception ex)
                    {
                        task.Result = ex.ToString();
                        task.ProcessedUtc = DateTime.UtcNow;
                        task.HasPassed = false;
                        dataLayer.SaveDaemonTask(task);
                        daemonProcesses.TaskDone(task);
                    }
                }
            }
            finally 
            {
                daemonProcesses.ClearActive(this);
            }
        }

        #endregion
    }
}
