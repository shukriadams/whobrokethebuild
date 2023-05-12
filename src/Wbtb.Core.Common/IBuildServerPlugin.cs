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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="remoteBuildId"></param>
        /// <returns></returns>
        IEnumerable<User> GetUsersInBuild(BuildServer contextServer, string remoteBuildId);

        /// <summary>
        /// Creates a build record in db for all builds found under a given job. This will be called periodically.
        /// Note that Import builds, logs, user info etc is spread over multiple methods to limite the number of 
        /// different subsystems needed or under load at one time.
        /// </summary>
        /// <param name="job"></param>
        /// /// <param name="take">Max number of builds to look back</param>
        BuildImportSummary ImportBuilds(Job job, int take);

        /// <summary>
        /// Import all builds.
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        BuildImportSummary ImportAllCachedBuilds(Job job);

        /// <summary>
        /// Imports logs for builds within a job. Logs are retrieved from the build system and permanently stored on the local filesystem.
        /// Once stored the build is marked as processed and will not have its log imported again.
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        IEnumerable<Build> ImportLogs(Job job);

        string GetEphemeralBuildLog(Build build);
    }
}
