﻿using System.Collections.Generic;
using Wbtb.Core.Common.Plugins;
using Wbtb.Core.Common.Plugins.Transmitters;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// Describes a type which interacts with a persistent data store read/writes of data objects.
    /// </summary>
    [PluginProxy(typeof(DataLayerPluginProxy))]
    [PluginBehaviour(true)]
    public interface IDataLayerPlugin: IReachable, IPlugin 
    {
        #region UTILITY

        object InitializeDatastore();

        #endregion

        #region BUILD SERVERS

        BuildServer SaveBuildServer(BuildServer buildServer);

        BuildServer GetBuildServerById(string id);

        BuildServer GetBuildServerByKey(string id);

        IEnumerable<BuildServer> GetBuildServers();

        bool DeleteBuildServer(BuildServer record);

        #endregion

        #region SOURCE SERVERS

        SourceServer SaveSourceServer(SourceServer sourceServer);

        SourceServer GetSourceServerById(string id);

        SourceServer GetSourceServerByKey(string id);

        IEnumerable<SourceServer> GetSourceServers();

        bool DeleteSourceServer(SourceServer record);

        #endregion

        #region JOB

        Job SaveJob(Job job);

        Job GetJobById(string id);

        Job GetJobByKey(string id);

        IEnumerable<Job> GetJobs();

        IEnumerable<Job> GetJobsByBuildServerId(string buildServerId);

        bool DeleteJob(Job record);

        /// <summary>
        /// Gets the ids of all builds which caused incidents in a given job, sorted in descending order of date
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        IEnumerable<string> GetIncidentIdsForJob(Job job);

        JobStats GetJobStats(Job job);
        
        /// <summary>
        /// Deletes all locally calculated data under a job - all data should be regeneratable by daemon processes.
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        int ResetJob(string jobId, bool hard);

        #endregion

        #region USERS

        User GetUserById(string id);

        User GetUserByKey(string id);

        User SaveUser(User user);

        IEnumerable<User> GetUsers();

        PageableData<User> PageUsers(int index, int pageSize);

        bool DeleteUser(User record);

        #endregion

        #region BUILD

        Build SaveBuild(Build build);

        Build GetBuildById(string id);

        Build GetBuildByKey(string jobId, string key);

        PageableData<Build> PageBuildsByJob(string jobId, int index, int pageSize);

        PageableData<Build> PageIncidentsByJob(string jobId, int index, int pageSize);

        PageableData<Build> PageBuildsByBuildAgent(string hostname, int index, int pageSize);

        bool DeleteBuild(Build record);

        IEnumerable<Build> GetBuildsWithNoLog(Job job);

        IEnumerable<Build> GetBuildsWithNoInvolvements(Job job);

        IEnumerable<Build> GetFailingBuildsWithoutIncident(Job job);

        Build GetLatestBuildByJob(Job job);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        Build GetBreakingBuildByJob(Job job);

        Build GetFirstPassingBuildAfterBuild(Build build);

        Build GetPreviousBuild(Build build);

        Build GetNextBuild(Build build);

        IEnumerable<Build> GetUnparsedBuildLogs(Job job);

        int ResetBuild(string buildId, bool hard);

        IEnumerable<Build> GetBuildsForPostProcessing(string jobid, string processorKey, int limit);

        #endregion

        #region BUILDFLAG

        BuildFlag SaveBuildFlag(BuildFlag flag);

        int IgnoreBuildFlagsForBuild(Build build, BuildFlags flag);

        int DeleteBuildFlagsForBuild(Build build, BuildFlags flag);

        IEnumerable<BuildFlag> GetBuildFlagsForBuild(Build build);

        PageableData<BuildFlag> PageBuildFlags(int index, int pageSize);

        BuildFlag GetBuildFlagById(string id);

        bool DeleteBuildFlag(BuildFlag record);

        #endregion

        #region BUILDLOG

        BuildLogParseResult SaveBuildLogParseResult(BuildLogParseResult buildLog);

        IEnumerable<BuildLogParseResult> GetBuildLogParseResultsByBuildId(string buildId);

        bool DeleteBuildLogParseResult(BuildLogParseResult record);

        #endregion

        #region BUILDINVOLVEMENT

        BuildInvolvement SaveBuildInvolement(BuildInvolvement buildInvolvement);

        BuildInvolvement GetBuildInvolvementById(string id);

        BuildInvolvement GetBuildInvolvementByRevisionCode(string jobId, string revisionCode);

        bool DeleteBuildInvolvement(BuildInvolvement record);

        IEnumerable<BuildInvolvement> GetBuildInvolvementsByBuild(string buildId);

        IEnumerable<BuildInvolvement> GetBuildInvolvementsWithoutMappedUser(string jobId);

        IEnumerable<BuildInvolvement> GetBuildInvolvementsWithoutMappedRevisions(string jobId);

        PageableData<BuildInvolvement> PageBuildInvolvementsByUserAndStatus(string userid, BuildStatus buildStatus, int index, int pageSize);

        IEnumerable<BuildInvolvement> GetBuildInvolvementByUserId(string userId);

        #endregion

        #region BUILD PROCESSOR

        BuildProcessor GetBuildProcessorById(string id);

        BuildProcessor SaveBuildProcessor(BuildProcessor buildProcessor);

        IEnumerable<BuildProcessor> GetBuildProcessorsByBuildId(string buildId);

        #endregion

        #region REVISION

        Revision SaveRevision(Revision revision);

        Revision GetRevisionById(string id);

        Revision GetRevisionByKey(string id);

        Revision GetNewestRevisionForBuild(string buildId);

        IEnumerable<Revision> GetRevisionByBuild(string buildId);

        bool DeleteRevision(Revision record);

        IEnumerable<Revision> GetRevisionsBySourceServer(string sourceServerId);

        #endregion

        #region SESSION

        Session SaveSession(Session session);

        Session GetSessionById(string id);

        IEnumerable<Session> GetSessionByUserId(string userid);

        bool DeleteSession(Session record);

        #endregion

        #region JOB DELTA

        Build GetLastJobDelta(string jobId);

        void SaveJobDelta(Build build);

        #endregion

        #region CONFIGURATIONSTATE

        ConfigurationState AddConfigurationState(ConfigurationState configurationState);

        ConfigurationState GetLatestConfigurationState();

        PageableData<ConfigurationState> PageConfigurationStates(int index, int pageSize);

        #endregion
    }
}
