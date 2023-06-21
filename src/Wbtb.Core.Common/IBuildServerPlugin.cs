using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    [PluginProxy(typeof(BuildServerPluginProxy))]
    [PluginBehaviour(allowMultiple: true)]
    public interface IBuildServerPlugin : IPlugin
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buildServer"></param>
        /// <returns></returns>
        ReachAttemptResult AttemptReach(BuildServer buildServer);

        void VerifyBuildServerConfig(BuildServer buildServer);

        /// <summary>
        /// Generates a canonical url for a given build on the given CI platform instance
        /// </summary>
        /// <param name="build"></param>
        /// <returns></returns>
        string GetBuildUrl(BuildServer contextServer, Build build);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        void AttemptReachJob(BuildServer buildServer, Job job);

        /// <summary>
        /// Generates a collection of objects representing jobs on the given CI server. Objects are not in database,
        /// but Externald can be used to manually create local config. This is intended to be an administrative aid
        /// and will be used in CLI mode.
        /// </summary>
        /// <param name="buildServer"></param>
        /// <returns></returns>
        IEnumerable<string> ListRemoteJobsCanonical(BuildServer buildServer);

        IEnumerable<Build> GetAllCachedBuilds(Job job);

        void PollBuildsForJob(Job job);

        IEnumerable<Build> GetLatesBuilds(Job job, int take);

        Build TryUpdateBuild(Build build);

        /// <summary>
        /// Gets list of revision codes (from source control server) which are known to be in a build. This works
        /// only if the given build on the build server logically supports this kind of query. Builds that are run
        /// periodically typically don't, and will typically need to have their revisions inferred.
        /// </summary>
        /// <param name="build"></param>
        /// <returns></returns>
        IEnumerable<string> GetRevisionsInBuild(Build build);

        /// <summary>
        /// Imports logs for builds within a job. Logs are retrieved from the build system and permanently stored on the local filesystem.
        /// Once stored the build is marked as processed and will not have its log imported again.
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        Build ImportLog(Build build);

        string GetEphemeralBuildLog(Build build);
    }
}
