using Microsoft.Extensions.Logging;
using System;
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

        private readonly IDaemonProcessRunner _processRunner;

        private readonly Configuration _config;

        private readonly PluginProvider _pluginProvider;

        private readonly SimpleDI _di;

        #endregion

        #region CTORS

        public RevisionFromLogDaemon(ILogger log, IDaemonProcessRunner processRunner)
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
            _processRunner.Start(new DaemonWorkThreaded(this.WorkThreaded), tickInterval, this, DaemonTaskTypes.RevisionFromLog);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _processRunner.Dispose();
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
            CachePayload cacheLookup = cache.Get(this.GetType().Name, hash);
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

            cache.Write(this.GetType().Name, hash, revFromLog);
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

            logText = File.ReadAllText(build.LogPath);
            revisionCode = GetRevisionFromLog(logText, job.RevisionAtBuildRegex);


            if (string.IsNullOrEmpty(revisionCode))
            {
                task.Result = $"Could not read a revision code from log content. This might be due to an error with your revision regex {job.RevisionAtBuildRegex}, but it could be that the revision string was not written to the log.";
                return new DaemonTaskWorkResult();
            }

            Revision revisionInLog = sourceServerPlugin.GetRevision(sourceServer, revisionCode);
            if (revisionInLog == null)
                return new DaemonTaskWorkResult {ResultType =DaemonTaskWorkResultType.Failed, Description = $"Unable to retrieve revision details for {revisionCode}." };

            IList<Revision> revisionsToLink = new List<Revision>() { revisionInLog };

            // get previous build and if it has any revision on it, span the gap with current build
            Build previousBuild = dataRead.GetPreviousBuild(build);
            if (previousBuild != null)
            {
                // note : assumes previous build's revisions have been successfully resolved so their date is available
                Revision lastRevisionOnPreviousBuild = dataRead.GetNewestRevisionForBuild(previousBuild.Id);
                if (lastRevisionOnPreviousBuild != null)
                    revisionsToLink = revisionsToLink.Concat(sourceServerPlugin.GetRevisionsBetween(job, lastRevisionOnPreviousBuild.Code, revisionInLog.Code)).ToList();
            }

            foreach (Revision revision in revisionsToLink)
            {
                string revisionId;

                // if revision doesn't exist in db, add it
                Revision lookupRevision = dataRead.GetRevisionByKey(sourceServer.Id, revision.Code);
                if (lookupRevision == null)
                {
                    revision.SourceServerId = sourceServer.Id;
                    revisionId = dataWrite.SaveRevision(revision).Id;
                }
                else
                {
                    revisionId = lookupRevision.Id;
                }

                // check if revision involvement already exists
                BuildInvolvement buildInvolvement = dataRead.GetBuildInvolvementsByBuild(build.Id).FirstOrDefault(bi => bi.RevisionCode == revision.Code);

                // create build involvement for this revision
                if (buildInvolvement == null)
                {
                    buildInvolvement = dataWrite.SaveBuildInvolement(new BuildInvolvement
                    {
                        BuildId = build.Id,
                        RevisionCode = revision.Code,
                        InferredRevisionLink = revisionCode != revision.Code,
                        RevisionId = revisionId
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

                }
                else
                {
                    task.Result = $"Build involvement id {buildInvolvement.Id} already existed.";
                }
            }

            return new DaemonTaskWorkResult { };
        }

        #endregion
    }
}
