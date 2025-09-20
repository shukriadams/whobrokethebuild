using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly Logger _log;

        private readonly IDaemonTaskController _taskController;

        private readonly PluginProvider _pluginProvider;

        private readonly SimpleDI _di;

        #endregion

        #region CTORS

        public RevisionFromBuildServerDaemon(Logger log, IDaemonTaskController processRunner)
        {
            _log = log;
            _taskController = processRunner;

            _di = new SimpleDI();
            _pluginProvider = _di.Resolve<PluginProvider>();
        }

        #endregion

        #region METHODS

        public void Start(int tickInterval)
        {
            _taskController.WatchForAndRunTasksForDaemon(this, tickInterval, ProcessStages.RevisionFromBuildServer);
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
            BuildServer buildServer = dataRead.GetBuildServerByKey(job.BuildServer);
            IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
            SourceServer sourceServer = dataRead.GetSourceServerById(job.SourceServerId);
            ISourceServerPlugin sourceServerPlugin = _pluginProvider.GetByKey(sourceServer.Plugin) as ISourceServerPlugin;
            ReachAttemptResult reach = buildServerPlugin.AttemptReach(buildServer);

            if (!reach.Reachable)
            {
                _log.Warn(this, $"Buildserver {buildServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}");
                return new DaemonTaskWorkResult {ResultType=DaemonTaskWorkResultType.Blocked, Description = $"Buildserver {buildServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}" };
            }

            reach = sourceServerPlugin.AttemptReach(sourceServer);
            if (!reach.Reachable)
            {
                _log.Warn(this, $"Sourceserver {sourceServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}");
                return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Blocked, Description = $"Sourceserver {sourceServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}" };
            }


            BuildRevisionsRetrieveResult result = buildServerPlugin.GetRevisionsInBuild(build);


            // go back in time to some build with a revision in it that we can reference against
            Build previousBuild = build;
            IEnumerable<BuildInvolvement> buildInvolvementsInPreviousBuild = new BuildInvolvement[] { };

            while (true)
            {
                previousBuild = dataRead.GetPreviousBuild(previousBuild);

                // no more builds to search through 
                if (previousBuild == null)
                    break;

                // build must be pass or fail to count for history traversal
                if (!previousBuild.IsDefinitive())
                     continue;

                // check if previous build is still having its involvements calculated
                if (dataRead.GetDaemonTasksByBuild(previousBuild.Id).Any(t => t.ProcessedUtc == null && (t.Stage == (int)ProcessStages.RevisionFromBuildServer) || t.Stage == (int)ProcessStages.RevisionFromLog))
                    return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Blocked, Description = $"Previous build id {previousBuild.Id} still processing build involvements, waiting." };

                buildInvolvementsInPreviousBuild = dataRead.GetBuildInvolvementsByBuild(previousBuild.Id);
                if (buildInvolvementsInPreviousBuild.Any())
                    break;
            }

            IList<string> revisionsToLink = result.Revisions.ToList();

            if (revisionsToLink.Any() && buildInvolvementsInPreviousBuild.Any())
            {
                // take any revision known to be in the previous build, we can span with that and tidy up as we go
                IEnumerable<Revision> revsBetween = sourceServerPlugin.GetRevisionsBetween(job, buildInvolvementsInPreviousBuild.First().RevisionCode, revisionsToLink.First());
                revisionsToLink = revisionsToLink.Concat(revsBetween.Select(r => r.Code)).ToList();
            }

            // get build involvements already in this build
            IEnumerable<BuildInvolvement> buildInvolvementsInThisBuild = dataRead.GetBuildInvolvementsByBuild(build.Id);

            revisionsToLink = revisionsToLink.Distinct().ToList();

            foreach (string revisionCode in revisionsToLink)
            {
                BuildInvolvement buildInvolvement = buildInvolvementsInThisBuild.FirstOrDefault(bi => bi.RevisionCode == revisionCode);
                if (buildInvolvement != null)
                {
                    task.AppendResult($"Build involvement id {buildInvolvement.Id} already existed.");
                    continue;
                }

                // check if revision shows up in history of previous build, if so, we've overspanned, but that's ok, just ignore it
                if (buildInvolvementsInPreviousBuild.Any(bi => bi.RevisionCode == revisionCode))
                    continue;

                string biID = dataWrite.SaveBuildInvolement(new BuildInvolvement
                {
                    BuildId = build.Id,
                    RevisionCode = revisionCode
                }).Id;

                dataWrite.SaveDaemonTask(new DaemonTask
                {
                    Stage = (int)ProcessStages.RevisionLink,
                    BuildId = build.Id,
                    BuildInvolvementId = biID,
                    Src = this.GetType().Name
                });

                dataWrite.SaveDaemonTask(new DaemonTask
                {
                    Stage = (int)ProcessStages.UserLink,
                    BuildId = build.Id,
                    BuildInvolvementId = biID,
                    Src = this.GetType().Name
                });

                _log.Status(this, $"Linked revision {revisionCode} to build {build.Key} (id:{build.Id})");

            }

            task.AppendResult(result.Result);
            return new DaemonTaskWorkResult();
        }

        #endregion
    }
}
