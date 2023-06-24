using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Wbtb.Core.Common;
using Wbtb.Core.Web.Daemons;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Creates build involvements for a build using build log contents - this is for jobs where builds are not triggered
    /// by source code commits, but by timers. This works on jobs that have the feature enabled.
    /// </summary>
    public class BuildRevisionFromLogDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly ILogger _log;

        private readonly IDaemonProcessRunner _processRunner;

        private readonly Configuration _config;

        private readonly PluginProvider _pluginProvider;

        private readonly SimpleDI _di;

        #endregion

        #region CTORS

        public BuildRevisionFromLogDaemon(ILogger log, IDaemonProcessRunner processRunner)
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
        /// Tries to read a revision from a log file at a given path, using the given regex. exposed to make manual testing of
        /// regex possible for end users.
        /// </summary>
        /// <param name="logPath"></param>
        /// <param name="regex"></param>
        /// <returns></returns>
        public string GetRevisionFromLog(string logText, string regex)
        {
            Match match = new Regex(regex, RegexOptions.IgnoreCase & RegexOptions.Multiline).Match(logText);
            if (!match.Success || match.Groups.Count < 2)
                return string.Empty; 

            return match.Groups[1].Value;
        }

        private void Work()
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            IEnumerable<DaemonTask> tasks = dataLayer.GetPendingDaemonTasksByTask(DaemonTaskTypes.AddBuildRevisionsFromBuildLog.ToString());

            foreach (DaemonTask task in tasks)
            {
                Build build = dataLayer.GetBuildById(task.BuildId);
                Job job = dataLayer.GetJobById(build.JobId);
                BuildServer buildServer = dataLayer.GetBuildServerByKey(job.BuildServer);
                SourceServer sourceServer = dataLayer.GetSourceServerByKey(job.SourceServer);
                ISourceServerPlugin sourceServerPlugin = _pluginProvider.GetByKey(sourceServer.Plugin) as ISourceServerPlugin;
                IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
                ReachAttemptResult reach = buildServerPlugin.AttemptReach(buildServer);

                if (!reach.Reachable)
                {
                    _log.LogError($"Buildserver {buildServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}");
                    continue;
                }

                reach = sourceServerPlugin.AttemptReach(sourceServer);
                if (!reach.Reachable)
                {
                    _log.LogError($"Sourceserver {sourceServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}");
                    continue;
                }

                string logText;
                string revisionCode;

                try
                {
                    logText = File.ReadAllText(build.LogPath);
                    revisionCode = GetRevisionFromLog(logText, job.RevisionAtBuildRegex);

                }
                catch (Exception ex)
                {
                    task.Result = ex.ToString();
                    task.ProcessedUtc = DateTime.UtcNow;
                    task.HasPassed = false;
                    dataLayer.SaveDaemonTask(task);
                    continue;
                }

                try
                {
                    if (string.IsNullOrEmpty(revisionCode))
                    {
                        task.Result = "Failed to parse revision from log.";
                        task.ProcessedUtc = DateTime.UtcNow;
                        task.HasPassed = false;
                        dataLayer.SaveDaemonTask(task);
                        continue;
                    }

                    Revision revisionInLog = sourceServerPlugin.GetRevision(sourceServer, revisionCode);
                    if (revisionInLog == null)
                    {
                        task.Result = $"Unable to retrieve revision details for {revisionCode}.";
                        task.ProcessedUtc = DateTime.UtcNow;
                        task.HasPassed = false;
                        dataLayer.SaveDaemonTask(task);
                        continue;
                    }

                    IList<Revision> revisionsToLink = new List<Revision>() { revisionInLog };

                    // get previous build and if it has any revision on it, span the gap with current build
                    Build previousBuild = dataLayer.GetPreviousBuild(build);
                    if (previousBuild != null)
                    {
                        // note : assumes previous build's revisions have been successfully resolved so their date is available
                        Revision lastRevisionOnPreviousBuild = dataLayer.GetNewestRevisionForBuild(previousBuild.Id);
                        if (lastRevisionOnPreviousBuild != null)
                            revisionsToLink = revisionsToLink.Concat(sourceServerPlugin.GetRevisionsBetween(sourceServer, lastRevisionOnPreviousBuild.Code, revisionInLog.Code)).ToList();
                    }

                    foreach (Revision revision in revisionsToLink)
                    {
                        bool revisionNeedsResolving = false;
                        string revisionId;

                        // if revision doesn't exist in db, add it
                        Revision lookupRevision = dataLayer.GetRevisionByKey(sourceServer.Id, revision.Code);
                        if (lookupRevision == null)
                        {
                            revision.SourceServerId = sourceServer.Id;
                            revisionId = dataLayer.SaveRevision(revision).Id;
                            revisionNeedsResolving = true;
                        }
                        else 
                        {
                            revisionId = lookupRevision.Id;
                        }

                        // create build involvement for this revision
                        BuildInvolvement buildInvolvement = dataLayer.SaveBuildInvolement(new BuildInvolvement
                        {
                            BuildId = build.Id,
                            RevisionCode = revision.Code,
                            RevisionLinkStatus = LinkState.Completed,
                            InferredRevisionLink = revisionCode != revision.Code,
                            RevisionId = revisionId
                        });

                        if (revisionNeedsResolving)
                            dataLayer.SaveDaemonTask(new DaemonTask
                            {
                                BuildId = build.Id,
                                BuildInvolvementId = buildInvolvement.Id,
                                Src = this.GetType().Name,
                                TaskKey = DaemonTaskTypes.RevisionResolve.ToString(),
                                Order = 3,
                            });

                        task.ProcessedUtc = DateTime.UtcNow;
                        task.HasPassed = true;
                        dataLayer.SaveDaemonTask(task);
                    }
                }
                catch (Exception ex)
                {
                    dataLayer.SaveBuildFlag(new BuildFlag
                    {
                        BuildId = build.Id,
                        Flag = BuildFlags.LogHasNoRevision,
                        Description = $"Log parse failed with ex : {ex}"
                    });

                    Console.WriteLine($"Log parse failed with ex : {ex}");
                }

            }













            foreach (BuildServer cfgbuildServer in _config.BuildServers)
            {
                BuildServer buildServer = dataLayer.GetBuildServerByKey(cfgbuildServer.Key);
                // note : buildserver can be null if trying to run daemon before auto data injection has had time to run
                if (buildServer == null)
                    continue;

                IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
                foreach (Job cfgJob in buildServer.Jobs.Where(j => !string.IsNullOrEmpty(j.SourceServer) && !string.IsNullOrEmpty(j.RevisionAtBuildRegex)))
                {
                    try
                    {
                        Job jobInDatabase = dataLayer.GetJobByKey(cfgJob.Key);
                        SourceServer sourceServer = dataLayer.GetSourceServerById(jobInDatabase.SourceServerId);
                        ISourceServerPlugin sourceServerPlugin = _pluginProvider.GetByKey(sourceServer.Plugin) as ISourceServerPlugin;
                        IEnumerable<Build> builds = dataLayer.GetBuildsWithNoInvolvements(jobInDatabase)
                            .OrderBy(b => b.StartedUtc);

                        foreach (Build build in builds)
                        {

                        }
                    }
                    catch (Exception ex)
                    {
                        _log.LogError($"Unexpected error trying to import jobs/logs for {cfgJob.Key} from buildserver {buildServer.Key}: {ex}");
                    }
                }
            }
        }

        #endregion
    }
}
