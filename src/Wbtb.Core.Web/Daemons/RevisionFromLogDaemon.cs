using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Creates build involvements for a build using build log contents - this is for jobs where builds are not triggered
    /// by source code commits, but by timers. This works on jobs that have the feature enabled.
    /// </summary>
    public class RevisionFromLogDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly ILogger _log;

        private readonly IDaemonTaskController _taskController;

        private readonly Configuration _config;

        private readonly PluginProvider _pluginProvider;

        private readonly SimpleDI _di;

        #endregion

        #region CTORS

        public RevisionFromLogDaemon(ILogger log, IDaemonTaskController processRunner)
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
            _taskController.WatchForAndRunTasksForDaemon(new DaemonWorkThreaded(this.WorkThreaded), tickInterval, this, DaemonTaskTypes.RevisionFromLog);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _taskController.Dispose();
        }
        
        /// <summary>
        /// Tries to read a revision from a log file at a given path, using the given regex. exposed to make manual testing of
        /// regex possible for end users.
        /// </summary>
        /// <param name="logPath"></param>
        /// <param name="regex"></param>
        /// <returns></returns>
        public string GetRevisionFromLog(string logText, string regex)
        {
            SimpleDI di = new SimpleDI();
            Cache cache = di.Resolve<Cache>();
            string hash = Sha256.FromString(regex + logText);
            string revFromLog = null;
            CachePayload cacheLookup = cache.Get(TypeHelper.Name(this), hash);

            if (cacheLookup.Payload != null)
                return cacheLookup.Payload;

            Match match = new Regex(regex, RegexOptions.Singleline & RegexOptions.Compiled).Match(logText);
            if (!match.Success || match.Groups.Count < 2)
            {
                revFromLog = string.Empty;
            }
            else 
            {
                revFromLog = match.Groups[1].Value;
            }

            cache.Write(TypeHelper.Name(this), hash, revFromLog);
            return revFromLog;
        }

        private DaemonTaskWorkResult WorkThreaded(IDataPlugin dataRead, IDataPlugin dataWrite, DaemonTask task, Build build, Job job)
        {
            BuildServer buildServer = dataRead.GetBuildServerByKey(job.BuildServer);
            SourceServer sourceServer = dataRead.GetSourceServerById(job.SourceServerId);
            ISourceServerPlugin sourceServerPlugin = _pluginProvider.GetByKey(sourceServer.Plugin) as ISourceServerPlugin;
            IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
            ReachAttemptResult reach = buildServerPlugin.AttemptReach(buildServer);

            if (!reach.Reachable)
            {
                _log.LogError($"Buildserver {buildServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}");
                return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Blocked, Description = $"Buildserver {buildServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}" };
            }

            reach = sourceServerPlugin.AttemptReach(sourceServer);
            if (!reach.Reachable)
            {
                _log.LogError($"Sourceserver {sourceServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}");
                return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Blocked, Description = $"Sourceserver {sourceServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}" };
            }

            string logText;
            string revisionCode;

            if (string.IsNullOrEmpty(job.RevisionAtBuildRegex))
                return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Blocked, Description = "RevisionAtBuildRegex not set" };

            string logPath = Build.GetLogPath(_config, job, build);
            logText = File.ReadAllText(logPath);
            revisionCode = this.GetRevisionFromLog(logText, job.RevisionAtBuildRegex);

            if (string.IsNullOrEmpty(revisionCode))
            {
                task.Result = $"Could not read a revision code from log content. This might be due to an error with your revision regex {job.RevisionAtBuildRegex}, but it could be that the revision string was not written to the log.";
                return new DaemonTaskWorkResult();
            }

            // write revision to build if not yet saved
            if (build.RevisionInBuildLog != revisionCode)
            {
                build.RevisionInBuildLog = revisionCode;
                dataWrite.SaveBuild(build);
            }


            RevisionLookup revisionLookup = sourceServerPlugin.GetRevision(sourceServer, revisionCode);
            if (!revisionLookup.Success)
                return new DaemonTaskWorkResult {ResultType =DaemonTaskWorkResultType.Failed, Description = $"Unable to retrieve revision details for {revisionCode}." };

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
                    break;

                // check if previous build is still having its involvements calculated
                if (dataRead.GetDaemonTasksByBuild(previousBuild.Id).Any(t => t.ProcessedUtc == null && (t.Stage == (int)DaemonTaskTypes.RevisionFromLog|| t.Stage == (int)DaemonTaskTypes.RevisionFromBuildServer)))
                    return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Blocked, Description = $"Previous build id {previousBuild.Id} still processing build involvements, waiting." };

                buildInvolvementsInPreviousBuild = dataRead.GetBuildInvolvementsByBuild(previousBuild.Id);
                if (buildInvolvementsInPreviousBuild.Any())
                    break;
            }

            IList<Revision> revisionsToLink = new List<Revision>() { revisionLookup.Revision };

            if (buildInvolvementsInPreviousBuild.Any())
            {
                // take any revision known to be in the previous build, we can span with that and tidy up as we go
                revisionsToLink = revisionsToLink.Concat(sourceServerPlugin.GetRevisionsBetween(job, buildInvolvementsInPreviousBuild.First().RevisionCode, revisionLookup.Revision.Code)).ToList();
            }

            // get build involvements already in this build
            IEnumerable<BuildInvolvement> buildInvolvementsInThisBuild = dataRead.GetBuildInvolvementsByBuild(build.Id);

            IEnumerable<string> revisionsIdsToLink = revisionsToLink.Select(r => r.Code).Distinct();

            foreach (string revisionIdToLink in revisionsIdsToLink)
            {
                BuildInvolvement buildInvolvement = buildInvolvementsInThisBuild.FirstOrDefault(bi => bi.RevisionCode == revisionIdToLink);
                if (buildInvolvement != null) 
                {
                    task.Result = $"Build involvement id {buildInvolvement.Id} already existed.";
                    continue;
                }
    
                // check if revision shows up in history of previous build, if so, we've overspanned, but that's ok, just ignore it
                if (buildInvolvementsInPreviousBuild.Any(bi => bi.RevisionCode == revisionIdToLink))
                    continue;

                // create build involvement for this revision
                buildInvolvement = dataWrite.SaveBuildInvolement(new BuildInvolvement
                {
                    BuildId = build.Id,
                    RevisionCode = revisionIdToLink,
                    InferredRevisionLink = revisionCode != revisionIdToLink
                });

                dataWrite.SaveDaemonTask(new DaemonTask
                {
                    BuildId = build.Id,
                    BuildInvolvementId = buildInvolvement.Id,
                    Src = this.GetType().Name,
                    Stage = (int)DaemonTaskTypes.RevisionLink
                });

                dataWrite.SaveDaemonTask(new DaemonTask
                {
                    BuildId = build.Id,
                    Src = this.GetType().Name,
                    BuildInvolvementId = buildInvolvement.Id,
                    Stage = (int)DaemonTaskTypes.UserLink
                });

                ConsoleHelper.WriteLine(this, $"Linked revision {revisionIdToLink} to build {build.Key} (id:{build.Id})");
            }

            return new DaemonTaskWorkResult { };
        }

        #endregion
    }
}
