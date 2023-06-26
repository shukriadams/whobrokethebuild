using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;
using Wbtb.Core.Web.Daemons;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Imports full revision objects from source control system, creates them in DB, and links them to buildinvolvements
    /// where applicable.
    /// </summary>
    public class RevisionResolveDaemon : IWebDaemon
    {
        #region FIELDS

        private ILogger _log;

        private IDaemonProcessRunner _processRunner;

        private readonly Configuration _config;
        
        private readonly PluginProvider _pluginProvider;

        public static int TaskGroup = 2;

        #endregion

        #region CTORS

        public RevisionResolveDaemon(ILogger log, IDaemonProcessRunner processRunner)
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

        /// <summary>
        /// Daemon's main work method
        /// </summary>
        private void Work()
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            IEnumerable<DaemonTask> tasks = dataLayer.GetPendingDaemonTasksByTask(DaemonTaskTypes.RevisionResolve.ToString());
            foreach (DaemonTask task in tasks)
            {
                Build build = dataLayer.GetBuildById(task.BuildId);
                if (dataLayer.DaemonTasksBlocked(build.Id, TaskGroup))
                    continue;

                Job job = dataLayer.GetJobById(build.JobId);
                BuildInvolvement buildInvolvement = dataLayer.GetBuildInvolvementById(task.BuildInvolvementId);
                SourceServer sourceServer = dataLayer.GetSourceServerByKey(job.SourceServer);
                ISourceServerPlugin sourceServerPlugin = _pluginProvider.GetByKey(sourceServer.Plugin) as ISourceServerPlugin;
                Revision revision = dataLayer.GetRevisionByKey(sourceServer.Id, buildInvolvement.RevisionCode);
               
                if (revision != null)
                {
                    task.HasPassed = true;
                    task.ProcessedUtc = DateTime.UtcNow;
                    task.Result = $"Revision {buildInvolvement.RevisionCode} already resolved";
                    dataLayer.SaveDaemonTask(task);
                    continue;
                }

                if (!sourceServerPlugin.AttemptReach(sourceServer).Reachable) 
                {
                    Console.WriteLine($"unable to reach source server \"{sourceServer.Name}\", waiting for later.");
                    continue;
                }

                revision = sourceServerPlugin.GetRevision(sourceServer, buildInvolvement.RevisionCode);
                if (revision == null)
                {
                    task.HasPassed = false;
                    task.ProcessedUtc = DateTime.UtcNow;
                    task.Result = $"Failed to resolve revision {buildInvolvement.RevisionCode} from source control server.";
                    dataLayer.SaveDaemonTask(task);
                    continue;
                }

                revision.SourceServerId = sourceServer.Id;
                revision =  dataLayer.SaveRevision(revision);

                buildInvolvement.RevisionId = revision.Id;
                dataLayer.SaveBuildInvolement(buildInvolvement);

                task.HasPassed = true;
                task.ProcessedUtc = DateTime.UtcNow;
                dataLayer.SaveDaemonTask(task);

            }
        }

        #endregion
    }
}
