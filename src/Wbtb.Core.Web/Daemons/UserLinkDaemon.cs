using Microsoft.Extensions.Logging;
using System;
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

        private Logger _log;

        private IDaemonTaskController _taskController;

        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        private readonly SimpleDI _di;

        #endregion

        #region CTORS

        public UserLinkDaemon(Logger log, IDaemonTaskController processRunner)
        {
            _log = log;
            _taskController = processRunner;

            _di = new SimpleDI();
            _config = _di.Resolve<Configuration>();
            _pluginProvider = _di.Resolve<PluginProvider>(); 

        }

        #endregion

        #region METHODS

        public void Start(int tickInterval)
        {
            _taskController.WatchForAndRunTasksForDaemon(this, tickInterval, ProcessStages.UserLink);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _taskController.Dispose();
        }

        void IWebDaemon.Work() 
        {
            throw new NotImplementedException();
        }

        DaemonTaskWorkResult IWebDaemon.WorkThreaded(IDataPlugin dataRead, IDataPlugin dataWrite, DaemonTask task, Build build, Job job)
        {
            BuildInvolvement buildInvolvement = dataRead.GetBuildInvolvementById(task.BuildInvolvementId);
            SourceServer sourceServer = dataRead.GetSourceServerByKey(job.SourceServer);
            Revision revision = dataRead.GetRevisionByKey(sourceServer.Id, buildInvolvement.RevisionCode);

            if (revision == null)
                return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Blocked, Description = $"Expected revision {buildInvolvement.RevisionCode} has not yet been resolved" };

            User matchingUser = _config.Users
                .FirstOrDefault(r => r.SourceServerIdentities
                    .Any(r => r.Name == revision.User));

            User userInDatabase = null;
            if (matchingUser != null)
                userInDatabase = dataRead.GetUserByKey(matchingUser.Key);

            if (userInDatabase == null)
                return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Failed, Description = $"User {revision.User} for buildinvolvement does not exist. Add user and rerun import" };

            buildInvolvement.MappedUserId = userInDatabase.Id;
            dataWrite.SaveBuildInvolement(buildInvolvement);

            _log.Status(this, $"Linked user {userInDatabase.Name} to build {build.Key} (id:{build.Id})");

            return new DaemonTaskWorkResult();
        }

        #endregion
    }
}
