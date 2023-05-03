using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Wbtb.Core.Common;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Creates build involvements for a build using build log contents - this is for jobs where builds are not triggered
    /// by source code commits, but by timers. This works on jobs that have the feature enabled.
    /// </summary>
    public class BuildRevisionFromLogDaemon : IWebDaemon
    {
        #region FIELDS

        private ILogger<BuildRevisionFromLogDaemon> _log;

        private IDaemonProcessRunner _processRunner;

        private readonly Config _config;

        private readonly PluginProvider _pluginProvider;

        #endregion

        #region CTORS

        public BuildRevisionFromLogDaemon(ILogger<BuildRevisionFromLogDaemon> log, IDaemonProcessRunner processRunner)
        {
            _log = log;
            _processRunner = processRunner;

            SimpleDI di = new SimpleDI();
            _config = di.Resolve<Config>();
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
        /// Tries to read a revision from a log file at a given path, using the given regex. exposed to make manual testing of
        /// regex possible for end users.
        /// </summary>
        /// <param name="logPath"></param>
        /// <param name="regex"></param>
        /// <returns></returns>
        public string GetRevision(string logText, string regex)
        {
            Match match = new Regex(regex, RegexOptions.IgnoreCase & RegexOptions.Multiline).Match(logText);
            if (!match.Success || match.Groups.Count < 2)
                return string.Empty; 

            return match.Groups[1].Value;
        }

        private void Work()
        {
            IDataLayerPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataLayerPlugin>();

            // start daemons - this should be folded into start
            foreach (BuildServer cfgbuildServer in _config.BuildServers.Where(b => b.Enable))
            {
                BuildServer buildServer = dataLayer.GetBuildServerByKey(cfgbuildServer.Key);
                // note : buildserver can be null if trying to run daemon before auto data injection has had time to run
                if (buildServer == null)
                    continue;

                IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
                foreach (Job cfgJob in buildServer.Jobs.Where(j => !string.IsNullOrEmpty(j.RevisionAtBuildRegex)))
                {
                    try
                    {
                        Job job = dataLayer.GetJobByKey(cfgJob.Key);
                        SourceServer sourceServer = dataLayer.GetSourceServerById(job.SourceServerId);
                        ISourceServerPlugin sourceServerPlugin = _pluginProvider.GetByKey(sourceServer.Plugin) as ISourceServerPlugin;
                        IEnumerable<Build> builds = dataLayer.GetBuildsWithNoInvolvements(job)
                            .OrderBy(b => b.StartedUtc);

                        foreach (Build build in builds)
                        {
                            try 
                            { 
                                string logText = buildServerPlugin.GetEphemeralBuildLog(build);
                                string revisionCode = GetRevision(logText, job.RevisionAtBuildRegex);
                                if (string.IsNullOrEmpty(revisionCode))
                                { 
                                    if (build.Status == BuildStatus.Aborted || build.Status == BuildStatus.Failed || build.Status == BuildStatus.Passed)
                                    {
                                        // if no revisioncode detected and build has already clearly finished, give up trying to read revision from build.
                                        // Else, try again later.
                                        dataLayer.SaveBuildFlag(new BuildFlag
                                        {
                                            BuildId = build.Id,
                                            Flag = BuildFlags.LogHasNoRevision,
                                            Description = "Log likely has no revision indicator in it, abandoning processing"
                                        });
                                        Console.WriteLine($"Cannot parse revision from log for build {build.Identifier}");
                                    }

                                    continue;
                                }

                                Revision revisionAtBuildTime = sourceServerPlugin.GetRevision(sourceServer, revisionCode);

                                if (revisionAtBuildTime == null)
                                    continue;

                                IList<Revision> revisionsToLink = new List<Revision>();
                                revisionsToLink.Add(revisionAtBuildTime);

                                if (job.RevisionScrapeSpanBuilds)
                                { 
                                    // get previous build and if it has any revision on it, span the gap with current build
                                    Build previousBuild = dataLayer.GetPreviousBuild(build);
                                    if (previousBuild != null)
                                    {
                                        Revision lastRevisionOnPreviousBuild = dataLayer.GetNewestRevisionForBuild(previousBuild.Id);

                                        if (lastRevisionOnPreviousBuild != null)
                                            revisionsToLink.Concat(sourceServerPlugin.GetRevisionsBetween(sourceServer, lastRevisionOnPreviousBuild.Code, revisionAtBuildTime.Code));
                                    }
                                }

                                foreach (Revision revision in revisionsToLink){
                                    if (dataLayer.GetRevisionByKey(revision.Code) == null){
                                        revision.SourceServerId = sourceServer.Id;
                                        dataLayer.SaveRevision(revision);
                                    }

                                    dataLayer.SaveBuildInvolement(new BuildInvolvement{
                                        BuildId = build.Id,
                                        RevisionCode = revision.Code,
                                        RevisionLinkStatus = LinkState.Completed,
                                        RevisionId = revision.Id
                                    });
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
