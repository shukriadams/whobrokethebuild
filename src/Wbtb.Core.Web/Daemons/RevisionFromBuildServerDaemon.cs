using Microsoft.Extensions.Logging;
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
            _processRunner.Start(new DaemonWorkThreaded(this.WorkThreaded), tickInterval, this, DaemonTaskTypes.RevisionFromBuildServer);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _processRunner.Dispose();
        }

        private DaemonTaskWorkResult WorkThreaded(IDataPlugin dataRead, IDataPlugin dataWrite, DaemonTask task, Build build, Job job)
        {
            BuildServer buildServer = dataRead.GetBuildServerByKey(job.BuildServer);
            IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
            SourceServer sourceServer = dataRead.GetSourceServerById(job.SourceServerId);
            ISourceServerPlugin sourceServerPlugin = _pluginProvider.GetByKey(sourceServer.Plugin) as ISourceServerPlugin;
            ReachAttemptResult reach = buildServerPlugin.AttemptReach(buildServer);

            if (!reach.Reachable)
            {
                _log.LogError($"Buildserver {buildServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}");
                return new DaemonTaskWorkResult {ResultType=DaemonTaskWorkResultType.Blocked, Description = $"Buildserver {buildServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}" };
            }

            reach = sourceServerPlugin.AttemptReach(sourceServer);
            if (!reach.Reachable)
            {
                _log.LogError($"Sourceserver {sourceServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}");
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
                if (dataRead.GetDaemonTasksByBuild(previousBuild.Id).Any(t => t.ProcessedUtc == null && (t.Stage == (int)DaemonTaskTypes.RevisionFromBuildServer) || t.Stage == (int)DaemonTaskTypes.RevisionFromLog))
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
                    task.Result = $"Build involvement id {buildInvolvement.Id} already existed.";
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

                ConsoleHelper.WriteLine(this, $"Linked revision {revisionCode} to build {build.Key} (id:{build.Id})");

            }

            task.Result = result.Result;
            return new DaemonTaskWorkResult();
        }

        #endregion
    }
}
