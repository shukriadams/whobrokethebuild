using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Adds revisions in build, assuming job supports revision-in-build lookup at build server. If not, must read revisions in build from 
    /// log, which has its own daemon.
    /// </summary>
    public class RevisionFromBuildServerDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly ILogger _log;

        private readonly IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        private readonly SimpleDI _di;

        #endregion

        #region CTORS

        public RevisionFromBuildServerDaemon(ILogger log, IDaemonProcessRunner processRunner)
        {
            _log = log;
            _processRunner = processRunner;

            _di = new SimpleDI();
            _config = _di.Resolve<Configuration>();
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
            IEnumerable<DaemonTask> tasks = dataRead.GetPendingDaemonTasksByTask((int)DaemonTaskTypes.RevisionFromBuildServer);
            DaemonTaskProcesses daemonProcesses = _di.Resolve<DaemonTaskProcesses>();

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

                        daemonProcesses.MarkActive(task, $"Task : {task.Id}, Build {build.Id}");

                        if (!reach.Reachable)
                        {
                            _log.LogError($"Buildserver {buildServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}");
                            daemonProcesses.MarkBlocked(task, $"Buildserver {buildServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}");
                            continue;
                        }

                        // this daemon's tasks are not blocked by preceeding tasks

                        dataWrite.TransactionStart();

                        BuildRevisionsRetrieveResult result = buildServerPlugin.GetRevisionsInBuild(build);
                        foreach (string revisionCode in result.Revisions)
                        {
                            string biID = dataWrite.SaveBuildInvolement(new BuildInvolvement
                            {
                                BuildId = build.Id,
                                RevisionCode = revisionCode
                            }).Id;

                            dataWrite.SaveDaemonTask(new DaemonTask
                            {
                                Stage = (int)DaemonTaskTypes.RevisionLink,
                                BuildId = build.Id,
                                BuildInvolvementId = biID,
                                Src = this.GetType().Name
                            });

                            dataWrite.SaveDaemonTask(new DaemonTask
                            {
                                Stage = (int)DaemonTaskTypes.UserLink,
                                BuildId = build.Id,
                                BuildInvolvementId = biID,
                                Src = this.GetType().Name
                            });
                        }

                        task.HasPassed = result.Success;
                        task.ProcessedUtc = DateTime.UtcNow;
                        task.Result = result.Result;
                        dataWrite.SaveDaemonTask(task);
                        dataWrite.TransactionCommit();
                        daemonProcesses.MarkDone(task);
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
