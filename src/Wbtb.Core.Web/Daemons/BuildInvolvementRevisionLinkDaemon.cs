using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Imports full revision objects from source control system, creates them in DB, and links them to buildinvolvements
    /// where applicable.
    /// </summary>
    public class BuildInvolvementRevisionResolveDaemon : IWebDaemon
    {
        #region FIELDS

        private ILogger _log;

        private IDaemonProcessRunner _processRunner;

        private readonly Configuration _config;
        
        private readonly PluginProvider _pluginProvider;
        
        private readonly SimpleDI _di;

        #endregion

        #region CTORS

        public BuildInvolvementRevisionResolveDaemon(ILogger log, IDaemonProcessRunner processRunner)
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
            IEnumerable<DaemonTask> tasks = dataRead.GetPendingDaemonTasksByTask((int)DaemonTaskTypes.RevisionLink);
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
                            daemonProcesses.AddActive(this, $"Task : {task.Id}, Build {build.Id}");

                            IEnumerable<DaemonTask> blocking = dataRead.DaemonTasksBlocked(build.Id, (int)DaemonTaskTypes.RevisionLink);
                            if (blocking.Any())
                            {
                                daemonProcesses.TaskBlocked(task, this, blocking);
                                continue;
                            }

                            Job job = dataRead.GetJobById(build.JobId);
                            BuildInvolvement buildInvolvement = dataRead.GetBuildInvolvementById(task.BuildInvolvementId);
                            SourceServer sourceServer = dataRead.GetSourceServerByKey(job.SourceServer);
                            ISourceServerPlugin sourceServerPlugin = _pluginProvider.GetByKey(sourceServer.Plugin) as ISourceServerPlugin;
                            Revision revision = dataRead.GetRevisionByKey(sourceServer.Id, buildInvolvement.RevisionCode);

                            if (!sourceServerPlugin.AttemptReach(sourceServer).Reachable)
                            {
                                Console.WriteLine($"unable to reach source server \"{sourceServer.Name}\", waiting for later.");
                                daemonProcesses.TaskBlocked(task, "source server down");
                                continue;
                            }

                            dataWrite.TransactionStart();

                            revision = sourceServerPlugin.GetRevision(sourceServer, buildInvolvement.RevisionCode);
                            if (revision == null)
                            {
                                task.HasPassed = false;
                                task.ProcessedUtc = DateTime.UtcNow;
                                task.Result = $"Failed to resolve revision {buildInvolvement.RevisionCode} from source control server.";
                                dataWrite.SaveDaemonTask(task);
                                dataWrite.TransactionCommit();
                                daemonProcesses.TaskDone(task);
                                continue;
                            }

                            revision.SourceServerId = sourceServer.Id;
                            revision = dataWrite.SaveRevision(revision);

                            buildInvolvement.RevisionId = revision.Id;
                            dataWrite.SaveBuildInvolement(buildInvolvement);

                            task.HasPassed = true;
                            task.ProcessedUtc = DateTime.UtcNow;
                            dataWrite.SaveDaemonTask(task);
                            dataWrite.TransactionCommit();
                            daemonProcesses.TaskDone(task);
                        }
                        catch (WriteCollisionException ex)
                        {
                            dataWrite.TransactionCancel();
                            _log.LogWarning($"Write collision trying to process task {task.Id}, trying again later");
                        }
                        catch (Exception ex)
                        {
                            dataWrite.TransactionCancel();

                            task.HasPassed = false;
                            task.ProcessedUtc = DateTime.UtcNow;
                            task.Result = ex.ToString();
                            dataWrite.SaveDaemonTask(task);
                            daemonProcesses.TaskDone(task);
                        }
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
