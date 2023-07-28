using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

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
            IDataPlugin dataRead = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            IEnumerable<DaemonTask> tasks = dataRead.GetPendingDaemonTasksByTask((int)DaemonTaskTypes.LogImport);
            TaskDaemonProcesses daemonProcesses = _di.Resolve<TaskDaemonProcesses>();

            try
            {
                foreach (DaemonTask task in tasks)
                {
                    using (IDataPlugin dataWrite = _pluginProvider.GetFirstForInterface<IDataPlugin>()) 
                    {
                        try
                        {
                            Build build = dataRead.GetBuildById(task.BuildId);
                            Job job = dataRead.GetJobById(build.JobId);
                            BuildServer buildServer = dataRead.GetBuildServerByKey(job.BuildServer);
                            IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
                            ReachAttemptResult reach = buildServerPlugin.AttemptReach(buildServer);

                            daemonProcesses.AddActive(this, $"Task : {task.Id}, Build {build.Id}");

                            if (!reach.Reachable)
                            {
                                _log.LogError($"Buildserver {buildServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}");
                                daemonProcesses.TaskBlocked(task, $"Buildserver {buildServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}");
                                continue;
                            }

                            IEnumerable<DaemonTask> blocking = dataRead.DaemonTasksBlocked(build.Id, (int)DaemonTaskTypes.LogImport);
                            if (blocking.Any())
                            {
                                daemonProcesses.TaskBlocked(task, this, blocking);
                                continue;
                            }

                            BuildLogRetrieveResult result = buildServerPlugin.ImportLog(build);
                            task.Result = result.Result;
                            task.ProcessedUtc = DateTime.UtcNow;

                            dataWrite.TransactionStart();

                            if (!result.Success)
                            {
                                task.HasPassed = false;
                                dataWrite.SaveDaemonTask(task);
                                dataWrite.TransactionCommit();
                                daemonProcesses.TaskDone(task);
                                continue;
                            }

                            build.LogPath = result.BuildLogPath;
                            dataWrite.SaveBuild(build);

                            task.HasPassed = true;
                            dataWrite.SaveDaemonTask(task);
                            daemonProcesses.TaskDone(task);

                            // create tasks for next stage
                            foreach (string logparser in job.LogParsers)
                                dataWrite.SaveDaemonTask(new DaemonTask
                                {
                                    BuildId = build.Id,
                                    Src = this.GetType().Name,
                                    Args = logparser,
                                    Stage = (int)DaemonTaskTypes.LogParse
                                });

                            // build revision requires source controld
                            if (!string.IsNullOrEmpty(job.RevisionAtBuildRegex) && !string.IsNullOrEmpty(job.SourceServerId))
                                dataWrite.SaveDaemonTask(new DaemonTask
                                {
                                    BuildId = build.Id,
                                    Src = this.GetType().Name,
                                    Stage = (int)DaemonTaskTypes.RevisionFromLog
                                });

                            dataWrite.TransactionCommit();
                        }
                        catch (WriteCollisionException ex)
                        {
                            dataWrite.TransactionCancel();
                            _log.LogWarning($"Write collision trying to process task {task.Id}, trying again later");
                        }
                        catch (Exception ex)
                        {
                            dataWrite.TransactionCancel();

                            task.Result = ex.ToString();
                            task.ProcessedUtc = DateTime.UtcNow;
                            task.HasPassed = false;
                            dataWrite.SaveDaemonTask(task);
                            daemonProcesses.TaskDone(task);
                        }
                    } // using
                } // foreach
            }
            finally 
            {
                daemonProcesses.ClearActive(this);
            }
        }

        #endregion
    }
}
