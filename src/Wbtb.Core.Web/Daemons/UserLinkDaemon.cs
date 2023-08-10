using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Daemon for connecting a defined and mapped WBTB user to the string username associated with a source control revision
    /// </summary>
    public class UserLinkDaemon : IWebDaemon
    {
        #region FIELDS

        private ILogger _log;

        private IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        private readonly SimpleDI _di;

        #endregion

        #region CTORS

        public UserLinkDaemon(ILogger log, IDaemonProcessRunner processRunner)
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

        private void Work()
        {
            IDataPlugin dataRead = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            IEnumerable<DaemonTask> tasks = dataRead.GetPendingDaemonTasksByTask((int)DaemonTaskTypes.UserLink);
            DaemonTaskProcesses daemonProcesses = _di.Resolve<DaemonTaskProcesses>();

            foreach (DaemonTask task in tasks)
            {
                using (IDataPlugin dataWrite = _pluginProvider.GetFirstForInterface<IDataPlugin>())
                {
                    try
                    {
                        Build build = dataRead.GetBuildById(task.BuildId);
                        daemonProcesses.MarkActive(task, this, build);

                        IEnumerable<DaemonTask> blocking = dataRead.DaemonTasksBlocked(build.Id, (int)DaemonTaskTypes.UserLink);
                        if (blocking.Any())
                        {
                            daemonProcesses.MarkBlocked(task, this, build, blocking);
                            continue;
                        }

                        Job job = dataRead.GetJobById(build.JobId);
                        BuildInvolvement buildInvolvement = dataRead.GetBuildInvolvementById(task.BuildInvolvementId);
                        SourceServer sourceServer = dataRead.GetSourceServerByKey(job.SourceServer);
                        Revision revision = dataRead.GetRevisionByKey(sourceServer.Id, buildInvolvement.RevisionCode);

                        if (revision == null)
                        {
                            task.ProcessedUtc = DateTime.UtcNow;
                            task.Result = $"Expected revision {buildInvolvement.RevisionCode} has not yet been resolved";
                            task.HasPassed = false;

                            daemonProcesses.MarkBlocked(task, this, build, task.Result);
                            continue;
                        }

                        User matchingUser = _config.Users
                            .FirstOrDefault(r => r.SourceServerIdentities
                                .Any(r => r.Name == revision.User));

                        User userInDatabase = null;
                        if (matchingUser != null)
                            userInDatabase = dataRead.GetUserByKey(matchingUser.Key);

                        dataWrite.TransactionStart();

                        if (userInDatabase == null)
                        {
                            task.ProcessedUtc = DateTime.UtcNow;
                            task.Result = $"User {revision.User} for buildinvolvement does not exist. Add user and rerun import";
                            task.HasPassed = false;
                            dataWrite.SaveDaemonTask(task);
                            dataWrite.TransactionCommit();
                            daemonProcesses.MarkDone(task);
                            continue;
                        }

                        buildInvolvement.MappedUserId = userInDatabase.Id;
                        dataWrite.SaveBuildInvolement(buildInvolvement);

                        task.ProcessedUtc = DateTime.UtcNow;
                        task.HasPassed = true;
                        dataWrite.SaveDaemonTask(task);
                        dataWrite.TransactionCommit();
                        daemonProcesses.MarkDone(task);
                    }
                    catch (WriteCollisionException ex)
                    {
                        dataWrite.TransactionCancel();
                        _log.LogWarning($"Write collision trying to process task {task.Id}, trying again later: {ex}");
                    }
                    catch (Exception ex)
                    {
                        dataWrite.TransactionCancel();

                        task.HasPassed = false;
                        task.ProcessedUtc = DateTime.UtcNow;
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
