using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;
using Wbtb.Core.Web.Daemons;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Daemon for connecting a defined and mapped WBTB user to the string username associated with a source control revision
    /// </summary>
    public class BuildInvolvementUserLinkDaemon : IWebDaemon
    {
        #region FIELDS

        private ILogger _log;

        private IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        public static int TaskGroup = 3;

        private readonly SimpleDI _di;

        #endregion

        #region CTORS

        public BuildInvolvementUserLinkDaemon(ILogger log, IDaemonProcessRunner processRunner)
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
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            IEnumerable<DaemonTask> tasks = dataLayer.GetPendingDaemonTasksByTask(DaemonTaskTypes.UserResolve.ToString());
            DaemonActiveProcesses activeItems = _di.Resolve<DaemonActiveProcesses>();

            try
            {
                foreach (DaemonTask task in tasks)
                {
                    try 
                    {
                        Build build = dataLayer.GetBuildById(task.BuildId);
                        activeItems.Add(this, $"Task : {task.Id}, Build {build.Id}");

                        if (dataLayer.DaemonTasksBlocked(build.Id, TaskGroup).Any())
                            continue;

                        Job job = dataLayer.GetJobById(build.JobId);
                        BuildInvolvement buildInvolvement = dataLayer.GetBuildInvolvementById(task.BuildInvolvementId);
                        SourceServer sourceServer = dataLayer.GetSourceServerByKey(job.SourceServer);
                        Revision revision = dataLayer.GetRevisionByKey(sourceServer.Id, buildInvolvement.RevisionCode);
                        if (revision == null)
                        {
                            task.ProcessedUtc = DateTime.UtcNow;
                            task.Result = $"Expected revision {buildInvolvement.RevisionCode} has not been resolved";
                            task.HasPassed = false;
                            continue;
                        }

                        User matchingUser = _config.Users
                            .FirstOrDefault(r => r.SourceServerIdentities
                                .Any(r => r.Name == revision.User));

                        User userInDatabase = null;
                        if (matchingUser != null)
                            userInDatabase = dataLayer.GetUserByKey(matchingUser.Key);

                        if (userInDatabase == null)
                        {
                            task.ProcessedUtc = DateTime.UtcNow;
                            task.Result = $"User {revision.User} for buildinvolvement does not exist. Add user and rerun import";
                            task.HasPassed = false;
                            dataLayer.SaveDaemonTask(task);
                            continue;
                        }

                        buildInvolvement.MappedUserId = userInDatabase.Id;
                        dataLayer.SaveBuildInvolement(buildInvolvement);

                        task.ProcessedUtc = DateTime.UtcNow;
                        task.HasPassed = true;
                        dataLayer.SaveDaemonTask(task);
                    }
                    catch(Exception ex) 
                    {
                        task.HasPassed = false;
                        task.ProcessedUtc = DateTime.UtcNow;
                        task.Result = ex.ToString();
                        dataLayer.SaveDaemonTask(task);
                    } 
                }
            }
            finally
            {
                activeItems.Clear(this);
            }
        }
        #endregion
    }
}
