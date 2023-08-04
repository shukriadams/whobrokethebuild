using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class BuildEndDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly ILogger _log;

        private readonly IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly SimpleDI _di;

        #endregion

        #region CTORS

        public BuildEndDaemon(ILogger log, IDaemonProcessRunner processRunner)
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
            DaemonTaskProcesses daemonProcesses = _di.Resolve<DaemonTaskProcesses>();
            IEnumerable<DaemonTask> tasks = dataRead.GetPendingDaemonTasksByTask((int)DaemonTaskTypes.BuildEnd);

            foreach (DaemonTask task in tasks)
            {
                using (IDataPlugin dataWrite = _pluginProvider.GetFirstForInterface<IDataPlugin>())
                {
                    try
                    {
                        Build build = dataRead.GetBuildById(task.BuildId);
                        Job job = dataRead.GetJobById(build.JobId);
                        BuildServer buildserver = dataRead.GetBuildServerById(job.BuildServerId);
                        IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildserver.Plugin) as IBuildServerPlugin;

                        daemonProcesses.MarkActive(task, $"Task : {task.Id}, Build {build.Id}");

                        build = buildServerPlugin.TryUpdateBuild(build);

                        // build still not done, contine and wait. Todo : Add forced time out on build here.
                        if (!build.EndedUtc.HasValue)
                        {
                            daemonProcesses.MarkBlocked(task, "Build not complete yet");
                            continue;
                        }

                        dataWrite.TransactionStart();
                        dataWrite.SaveBuild(build);

                        task.HasPassed = true;
                        task.ProcessedUtc = DateTime.UtcNow;
                        dataWrite.SaveDaemonTask(task);
                        daemonProcesses.MarkDone(task);

                        // create tasks for next stage
                        dataWrite.SaveDaemonTask(new DaemonTask
                        {
                            BuildId = build.Id,
                            Src = this.GetType().Name,
                            Stage = (int)DaemonTaskTypes.LogImport,
                        });

                        if (!string.IsNullOrEmpty(job.SourceServer) && string.IsNullOrEmpty(job.RevisionAtBuildRegex))
                            dataWrite.SaveDaemonTask(new DaemonTask
                            {
                                Stage = (int)DaemonTaskTypes.RevisionFromBuildServer,
                                Src = this.GetType().Name,
                                BuildId = build.Id
                            });

                        if (build.Status == BuildStatus.Failed)
                        {
                            dataWrite.SaveDaemonTask(new DaemonTask
                            {
                                BuildId = build.Id,
                                Src = this.GetType().Name,
                                Stage = (int)DaemonTaskTypes.IncidentAssign
                            });

                            if (job.PostProcessors.Any())
                                dataWrite.SaveDaemonTask(new DaemonTask
                                {
                                    BuildId = build.Id,
                                    Src = this.GetType().Name,
                                    Stage = (int)DaemonTaskTypes.PostProcess
                                });
                        }

                        dataWrite.TransactionCommit();
                    }
                    catch (WriteCollisionException ex) 
                    {
                        dataWrite.TransactionCancel();
                        _log.LogWarning($"Write collision trying to process task {task.Id}, trying again later : {ex}");
                    }
                    catch (Exception ex)
                    {
                        dataWrite.TransactionCancel();

                        task.ProcessedUtc = DateTime.UtcNow;
                        task.HasPassed = false;
                        task.Result = ex.ToString();
                        dataWrite.SaveDaemonTask(task);
                        daemonProcesses.MarkDone(task);
                    }
                    finally
                    {
                        daemonProcesses.ClearActive(task);
                    }
                }

            }
        }

        #endregion
    }
}
