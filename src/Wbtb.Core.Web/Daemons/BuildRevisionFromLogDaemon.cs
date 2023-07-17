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
            SimpleDI di = new SimpleDI();
            Cache cache = di.Resolve<Cache>();
            string hash = Sha256.FromString(regex + logText);
            string cacheLookup = cache.Get(this.GetType().Name, hash);
            if (cacheLookup != null)
                return cacheLookup;

            Match match = new Regex(regex, RegexOptions.Singleline & RegexOptions.Compiled).Match(logText);
            if (!match.Success || match.Groups.Count < 2)
            {
                cacheLookup = string.Empty;
            }
            else 
            {
                cacheLookup = match.Groups[1].Value;
            }

            cache.Write(this.GetType().Name, hash, cacheLookup);
            return cacheLookup;
        }

        private void Work()
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            IEnumerable<DaemonTask> tasks = dataLayer.GetPendingDaemonTasksByTask((int)DaemonTaskTypes.RevisionFromLog).Take(_config.MaxThreads);
            TaskDaemonProcesses daemonProcesses = _di.Resolve<TaskDaemonProcesses>();

            tasks.AsParallel().ForAll(delegate (DaemonTask task)
            {
                string processKey = string.Empty;

                try
                {
                    Build build = dataLayer.GetBuildById(task.BuildId);
                    Job job = dataLayer.GetJobById(build.JobId);
                    BuildServer buildServer = dataLayer.GetBuildServerByKey(job.BuildServer);
                    SourceServer sourceServer = dataLayer.GetSourceServerById(job.SourceServerId);
                    ISourceServerPlugin sourceServerPlugin = _pluginProvider.GetByKey(sourceServer.Plugin) as ISourceServerPlugin;
                    IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
                    ReachAttemptResult reach = buildServerPlugin.AttemptReach(buildServer);

                    processKey = $"{this.GetType().Name}_{task.Id}";

                    daemonProcesses.AddActive(processKey, $"Task : {task.Id}, Build {build.Id}");

                    if (!reach.Reachable)
                    {
                        _log.LogError($"Buildserver {buildServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}");
                        daemonProcesses.TaskBlocked(task, $"Buildserver {buildServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}");
                        return;
                    }
                    
                    IEnumerable<DaemonTask> blocking = dataLayer.DaemonTasksBlocked(build.Id, (int)DaemonTaskTypes.RevisionFromLog);
                    if (blocking.Any()) 
                    {
                        daemonProcesses.TaskBlocked(task, this, blocking);
                        return;
                    }

                    reach = sourceServerPlugin.AttemptReach(sourceServer);
                    if (!reach.Reachable)
                    {
                        _log.LogError($"Sourceserver {sourceServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}");
                        daemonProcesses.TaskBlocked(task, $"Sourceserver {sourceServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}");
                        return;
                    }

                    string logText;
                    string revisionCode;

                    if (string.IsNullOrEmpty(job.RevisionAtBuildRegex))
                        throw new Exception("RevisionAtBuildRegex) not set");

                    logText = File.ReadAllText(build.LogPath);
                    revisionCode = GetRevisionFromLog(logText, job.RevisionAtBuildRegex);

                    if (string.IsNullOrEmpty(revisionCode))
                    {
                        task.Result = $"Could not read a revision code from log content. This might be due to an error with your revision regex {job.RevisionAtBuildRegex}, but it could be that the revision string was not written to the log.";
                        task.ProcessedUtc = DateTime.UtcNow;
                        task.HasPassed = true;
                        dataLayer.SaveDaemonTask(task);
                        daemonProcesses.TaskDone(task);
                        return;
                    }

                    Revision revisionInLog = sourceServerPlugin.GetRevision(sourceServer, revisionCode);
                    if (revisionInLog == null)
                    {
                        task.Result = $"Unable to retrieve revision details for {revisionCode}.";
                        task.ProcessedUtc = DateTime.UtcNow;
                        task.HasPassed = false;
                        dataLayer.SaveDaemonTask(task);
                        daemonProcesses.TaskDone(task);
                        return;
                    }

                    IList<Revision> revisionsToLink = new List<Revision>() { revisionInLog };

                    // get previous build and if it has any revision on it, span the gap with current build
                    Build previousBuild = dataLayer.GetPreviousBuild(build);
                    if (previousBuild != null)
                    {
                        // note : assumes previous build's revisions have been successfully resolved so their date is available
                        Revision lastRevisionOnPreviousBuild = dataLayer.GetNewestRevisionForBuild(previousBuild.Id);
                        if (lastRevisionOnPreviousBuild != null)
                            revisionsToLink = revisionsToLink.Concat(sourceServerPlugin.GetRevisionsBetween(job, lastRevisionOnPreviousBuild.Code, revisionInLog.Code)).ToList();
                    }

                    foreach (Revision revision in revisionsToLink)
                    {
                        string revisionId;

                        // if revision doesn't exist in db, add it
                        Revision lookupRevision = dataLayer.GetRevisionByKey(sourceServer.Id, revision.Code);
                        if (lookupRevision == null)
                        {
                            revision.SourceServerId = sourceServer.Id;
                            revisionId = dataLayer.SaveRevision(revision).Id;
                        }
                        else
                        {
                            revisionId = lookupRevision.Id;
                        }

                        // check if revision involvement already exists
                        BuildInvolvement buildInvolvement = dataLayer.GetBuildInvolvementsByBuild(build.Id).FirstOrDefault(bi => bi.RevisionCode == revision.Code);

                        // create build involvement for this revision
                        if (buildInvolvement == null)
                        {
                            buildInvolvement = dataLayer.SaveBuildInvolement(new BuildInvolvement
                            {
                                BuildId = build.Id,
                                RevisionCode = revision.Code,
                                RevisionLinkStatus = LinkState.Completed,
                                InferredRevisionLink = revisionCode != revision.Code,
                                RevisionId = revisionId
                            });

                            dataLayer.SaveDaemonTask(new DaemonTask
                            {
                                BuildId = build.Id,
                                BuildInvolvementId = buildInvolvement.Id,
                                Src = this.GetType().Name,
                                Stage = (int)DaemonTaskTypes.RevisionLink
                            });

                            dataLayer.SaveDaemonTask(new DaemonTask
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

                        task.ProcessedUtc = DateTime.UtcNow;
                        task.HasPassed = true;
                        dataLayer.SaveDaemonTask(task);
                        daemonProcesses.TaskDone(task);
                    }
                }
                catch (Exception ex)
                {
                    task.ProcessedUtc = DateTime.UtcNow;
                    task.HasPassed = false;
                    task.Result = ex.ToString();
                    dataLayer.SaveDaemonTask(task);
                    daemonProcesses.TaskDone(task);
                }
                finally 
                {
                    daemonProcesses.ClearActive(processKey);
                }
            });
        }

        #endregion
    }
}
