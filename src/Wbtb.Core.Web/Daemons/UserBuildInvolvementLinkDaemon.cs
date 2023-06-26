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
    public class UserBuildInvolvementLinkDaemon : IWebDaemon
    {
        #region FIELDS

        private ILogger _log;

        private IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        public static int TaskGroup = 2;

        #endregion

        #region CTORS

        public UserBuildInvolvementLinkDaemon(ILogger log, IDaemonProcessRunner processRunner)
        {
            _log = log;
            _processRunner = processRunner;

            SimpleDI di = new SimpleDI();
            _config = di.Resolve<Configuration>();
            _pluginProvider = di.Resolve<PluginProvider>(); 

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
            foreach (DaemonTask task in tasks)
            {
                Build build = dataLayer.GetBuildById(task.BuildId);
                if (dataLayer.DaemonTasksBlocked(build.Id, TaskGroup))
                    continue;

                Job job = dataLayer.GetJobById(build.JobId);
                BuildInvolvement buildInvolvement = dataLayer.GetBuildInvolvementById(task.BuildInvolvementId);
                SourceServer sourceServer = dataLayer.GetSourceServerByKey(job.SourceServer);
                Revision revision = dataLayer.GetRevisionByKey(sourceServer.Id, buildInvolvement.RevisionCode);

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
        }
        #endregion
    }
}
