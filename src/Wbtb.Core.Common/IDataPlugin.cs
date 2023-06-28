﻿using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// Describes a type which interacts with a persistent data store read/writes of data objects.
    /// </summary>
    [PluginProxy(typeof(DataPluginProxy))]
    [PluginBehaviour(allowMultiple: false)]
    public interface IDataPlugin: IReachable, IPlugin 
    {
        #region UTILITY

        int InitializeDatastore();

        int DestroyDatastore();

        int ClearAllTables();

        #endregion

        #region STOREITEM

        StoreItem SaveStore(StoreItem storeItem);

        StoreItem GetStoreItemByItem(string id);

        StoreItem GetStoreItemByKey(string key);

        bool DeleteStoreItem(StoreItem record);

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

        PageableData<Build> PageBuildsByJob(string jobId, int index, int pageSize, bool sortAscending);

        PageableData<Build> PageIncidentsByJob(string jobId, int index, int pageSize);

        PageableData<Build> PageBuildsByBuildAgent(string hostname, int index, int pageSize);

        IEnumerable<Build> GetBuildsByIncident(string incidentId);

        bool DeleteBuild(Build record);

        /// <summary>
        /// Gets the build started the delta relative to a given build. Returns the given build if that build started the delta. 
        /// </summary>
        /// <param name="build"></param>
        /// <returns></returns>
        Build GetDeltaBuildAtBuild(Build build);

        Build GetLatestBuildByJob(Job job);

        Build GetFirstPassingBuildAfterBuild(Build build);

        Build GetPreviousBuild(Build build);

        Build GetNextBuild(Build build);

        int ResetBuild(string buildId, bool hard);

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

        PageableData<BuildInvolvement> PageBuildInvolvementsByUserAndStatus(string userid, BuildStatus buildStatus, int index, int pageSize);

        IEnumerable<BuildInvolvement> GetBuildInvolvementByUserId(string userId);

        #endregion

        #region R_BuildLogParseResult_BuildInvolvement

        string ConnectBuildLogParseResultAndBuildBuildInvolvement(string buildLogParseResultId, string buildInvolvementId);

        bool SplitBuildLogParseResultAndBuildBuildInvolvement(string id);

        IEnumerable<string> GetBuildLogParseResultsForBuildInvolvement(string buildInvolvementId);

        #endregion

        #region DAEMONTASK

        DaemonTask SaveDaemonTask(DaemonTask daemonTask);

        DaemonTask GetDaemonTaskById(string id);

        bool DeleteDaemonTask(DaemonTask record);

        IEnumerable<DaemonTask> GetDaemonsTaskByBuild(string buildid);

        /// <summary>
        /// Gets block of oldest pending tasks
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        IEnumerable<DaemonTask> GetPendingDaemonTasksByTask(string task);

        bool DaemonTasksBlocked(string buildId, int order);

        PageableData<DaemonTask> PageDaemonTasks(int index, int pageSize, string orderBy = "", string filterBy = "");

        #endregion

        #region REVISION

        Revision SaveRevision(Revision revision);

        Revision GetRevisionById(string id);

        Revision GetRevisionByKey(string sourceServerId, string key);

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

        #endregion

        #region CONFIGURATIONSTATE

        ConfigurationState AddConfigurationState(ConfigurationState configurationState);

        ConfigurationState GetLatestConfigurationState();

        PageableData<ConfigurationState> PageConfigurationStates(int index, int pageSize);

        #endregion
    }
}
