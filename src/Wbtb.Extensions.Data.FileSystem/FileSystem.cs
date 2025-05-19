using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.FileSystem
{
    internal class FileSystem : Plugin, IDataPlugin
    {
        void IDisposable.Dispose()
        {

        }

        void IDataPlugin.TransactionStart()
        {

        }

        void IDataPlugin.TransactionCommit()
        {

        }

        void IDataPlugin.TransactionCancel()
        {

        }

        ConfigurationState? IDataPlugin.AddConfigurationState(ConfigurationState configurationState)
        {
            return null;
        }

        ReachAttemptResult IReachable.AttemptReach()
        {
            return new ReachAttemptResult {  Reachable = true };
        }

        bool IDataPlugin.DeleteBuild(Build record)
        {
            return false;
        }

        bool IDataPlugin.DeleteBuildInvolvement(BuildInvolvement record)
        {
            return false;
        }

        bool IDataPlugin.DeleteBuildLogParseResult(BuildLogParseResult record)
        {
            return false;
        }

        bool IDataPlugin.DeleteBuildServer(BuildServer record)
        {
            return false;
        }

        bool IDataPlugin.DeleteJob(Job record)
        {
            return false;
        }

        bool IDataPlugin.DeleteRevision(Revision record)
        {
            return false;
        }

        bool IDataPlugin.DeleteSession(Session record)
        {
            return false;
        }

        bool IDataPlugin.DeleteSourceServer(SourceServer record)
        {
            return false;
        }

        bool IDataPlugin.DeleteStoreItem(StoreItem record)
        {
            return false;
        }

        bool IDataPlugin.DeleteStoreItemWithKey(string key)
        {
            return false;
        }

        bool IDataPlugin.DeleteUser(User record)
        {
            return false;
        }

        Build IDataPlugin.GetBuildById(string id)
        {
            return new Build { };
        }

        Build IDataPlugin.GetPrecedingBuildInIncident(Build build)
        {
            return new Build { };
        }

        Incident IDataPlugin.GetIncident(string incidentId) 
        {
            return new Incident { }; 
        }

        Build IDataPlugin.GetBuildByUniquePublicIdentifier(string uniquePublicIdentifier)
        {
            return new Build { };
        }

        Build IDataPlugin.GetLatestPassOrFailBuildByJob(Job job) 
        {
            return new Build { };
        }

        Build IDataPlugin.GetBuildByKey(string jobId, string key)
        {
            return new Build { };
        }

        BuildInvolvement IDataPlugin.GetBuildInvolvementById(string id)
        {
            return new BuildInvolvement { };
        }

        BuildInvolvement IDataPlugin.GetBuildInvolvementByRevisionCode(string buildid, string revisionCode)
        {
            return new BuildInvolvement { };
        }

        IEnumerable<BuildInvolvement> IDataPlugin.GetBuildInvolvementByUserId(string userId)
        {
            return new BuildInvolvement[] { };
        }

        IEnumerable<BuildInvolvement> IDataPlugin.GetBuildInvolvementsByBuild(string buildId)
        {
            return new BuildInvolvement[] { };
        }

        IEnumerable<BuildLogParseResult> IDataPlugin.GetBuildLogParseResultsByBuildId(string buildId)
        {
            return new BuildLogParseResult[] { };
        }

        BuildServer IDataPlugin.GetBuildServerById(string id)
        {
            return new BuildServer { };
        }

        BuildServer IDataPlugin.GetBuildServerByKey(string key)
        {
            return new BuildServer { };
        }

        IEnumerable<BuildServer> IDataPlugin.GetBuildServers()
        {
            return new BuildServer[] { };
        }

        Build IDataPlugin.GetFirstPassingBuildAfterBuild(Build build)
        {
            return new Build { };
        }

        Build IDataPlugin.GetDeltaBuildAtBuild(Build build) 
        {
            return new Build { };
        }

        IEnumerable<string> IDataPlugin.GetIncidentIdsForJob(Job job, int count)
        {
            return new string[] { };
        }

        Build IDataPlugin.GetPreviousIncident(Build referenceBuild) 
        {
            return new Build { };
        }

        Build IDataPlugin.GetFixForIncident(Build incident) 
        {
            return new Build { };
        }

        Build IDataPlugin.GetBreakBuildFixed(Build fix)
        {
            return new Build { };
        }

        Job IDataPlugin.GetJobById(string id)
        {
            return new Job { };
        }

        Job IDataPlugin.GetJobByKey(string key)
        {
            return new Job { };
        }

        IEnumerable<Job> IDataPlugin.GetJobs()
        {
            return new Job[] { };
        }

        IEnumerable<Job> IDataPlugin.GetJobsByBuildServerId(string buildServerId)
        {
            return new Job[] { };
        }

        JobStats IDataPlugin.GetJobStats(Job job)
        {
            return new JobStats { };
        }

        Build IDataPlugin.GetLastJobDelta(string jobId)
        {
            return new Build { };
        }

        Build IDataPlugin.GetLatestBuildByJob(Job job)
        {
            return new Build { };
        }

        ConfigurationState IDataPlugin.GetLatestConfigurationState()
        {
            return new ConfigurationState { };
        }

        Revision IDataPlugin.GetNewestRevisionForBuild(string buildId)
        {
            return new Revision { };
        }

        Build IDataPlugin.GetNextBuild(Build build)
        {
            return new Build { };
        }

        Build IDataPlugin.GetPreviousBuild(Build build)
        {
            return new Build { };
        }

        IEnumerable<Revision> IDataPlugin.GetRevisionByBuild(string buildId)
        {
            return new Revision[] { };
        }

        Revision IDataPlugin.GetRevisionById(string id)
        {
            return new Revision { };
        }

        Revision IDataPlugin.GetRevisionByKey(string sourceServerId, string key)
        {
            return new Revision { };
        }

        IEnumerable<Revision> IDataPlugin.GetRevisionsBySourceServer(string sourceServerId)
        {
            return new Revision[] { };
        }

        Session IDataPlugin.GetSessionById(string id)
        {
            return new Session { };
        }

        IEnumerable<Session> IDataPlugin.GetSessionByUserId(string userid)
        {
            return new Session[] { };
        }

        SourceServer IDataPlugin.GetSourceServerById(string id)
        {
            return new SourceServer { };
        }

        SourceServer IDataPlugin.GetSourceServerByKey(string key)
        {
            return new SourceServer { };
        }

        IEnumerable<SourceServer> IDataPlugin.GetSourceServers()
        {
            return new SourceServer[] { };
        }

        StoreItem IDataPlugin.GetStoreItemByItem(string id)
        {
            return new StoreItem { };
        }

        StoreItem IDataPlugin.GetStoreItemByKey(string key)
        {
            return new StoreItem { };
        }

        User IDataPlugin.GetUserById(string id)
        {
            return new User { };
        }

        User IDataPlugin.GetUserByKey(string key)
        {
            return new User { };
        }

        IEnumerable<User> IDataPlugin.GetUsers()
        {
            return new User[] { };
        }

        int IDataPlugin.InitializeDatastore()
        {
            return 0;
        }

        int IDataPlugin.DestroyDatastore()
        {
            return 0;
        }

        PluginInitResult IPlugin.InitializePlugin()
        {
            return new PluginInitResult { Success = true };
        }

        PageableData<BuildInvolvement> IDataPlugin.PageBuildInvolvementsByUserAndStatus(string userid, BuildStatus buildStatus, int index, int pageSize)
        {
            return new PageableData<BuildInvolvement>(new BuildInvolvement[] { }, 0, 0, 0);
        }

        PageableData<Build> IDataPlugin.PageBuildsByBuildAgent(string hostname, int index, int pageSize)
        {
            return new PageableData<Build>(new Build[] { }, 0, 0, 0);
        }

        IEnumerable<Build> IDataPlugin.GetBuildsByIncident(string incidentId)
        {
            return new Build[] { };
        }

        PageableData<Build> IDataPlugin.PageBuildsByJob(string jobId, int index, int pageSize, bool sortAscending)
        {
            return new PageableData<Build>(new Build[] { }, 0, 0, 0);
        }

        PageableData<ConfigurationState> IDataPlugin.PageConfigurationStates(int index, int pageSize)
        {
            return new PageableData<ConfigurationState>(new ConfigurationState[] { }, 0, 0, 0);
        }

        PageableData<Build> IDataPlugin.PageIncidentsByJob(string jobId, int index, int pageSize)
        {
            return new PageableData<Build>(new Build[] { }, 0, 0, 0);
        }

        PageableData<User> IDataPlugin.PageUsers(int index, int pageSize)
        {
            return new PageableData<User>(new User[] { }, 0, 0, 0);
        }

        int IDataPlugin.ResetBuild(string buildId, bool hard)
        {
            return 0;
        }

        int IDataPlugin.ResetJob(string jobId, bool hard)
        {
            return 0;
        }

        Build IDataPlugin.SaveBuild(Build build)
        {
            return new Build { };
        }

        BuildInvolvement IDataPlugin.SaveBuildInvolement(BuildInvolvement buildInvolvement)
        {
            return new BuildInvolvement { };
        }

        BuildLogParseResult IDataPlugin.SaveBuildLogParseResult(BuildLogParseResult buildLog)
        {
            return new BuildLogParseResult { };
        }

        BuildServer IDataPlugin.SaveBuildServer(BuildServer buildServer)
        {
            return new BuildServer { };
        }

        Job IDataPlugin.SaveJob(Job job)
        {
            return new Job { };
        }

        Revision IDataPlugin.SaveRevision(Revision revision)
        {
            return new Revision { };
        }

        Session IDataPlugin.SaveSession(Session session)
        {
            return new Session { };
        }

        SourceServer IDataPlugin.SaveSourceServer(SourceServer sourceServer)
        {
            return new SourceServer { };
        }

        StoreItem IDataPlugin.SaveStore(StoreItem storeItem)
        {
            return new StoreItem { };
        }

        User IDataPlugin.SaveUser(User user)
        {
            return new User { };
        }

        int IDataPlugin.ClearAllTables() 
        {
            return 0;
        }

        #region R_BuildLogParseResult_BuildInvolvement

        string IDataPlugin.ConnectBuildLogParseResultAndBuildBuildInvolvement(string buildLogParseResultId, string buildInvolvementId)
        {
            return string.Empty;
        }

        bool IDataPlugin.SplitBuildLogParseResultAndBuildBuildInvolvement(string id)
        {
            return false;
        }

        IEnumerable<string> IDataPlugin.GetBuildLogParseResultsForBuildInvolvement(string buildInvolvementId)
        {
            return new string[] { };
        }

        #endregion

        #region DAEMONTASK

        DaemonTask IDataPlugin.SaveDaemonTask(DaemonTask daemonTask)
        {
            return new DaemonTask { };
        }

        DaemonTask IDataPlugin.GetDaemonTaskById(string id)
        {
            return new DaemonTask { };
        }

        bool IDataPlugin.DeleteDaemonTask(DaemonTask record)
        {
            return false;
        }

        IEnumerable<DaemonTask> IDataPlugin.GetDaemonTasksByBuild(string buildid)
        {
            return new DaemonTask[] { };
        }

        IEnumerable<DaemonTask> IDataPlugin.GetPendingDaemonTasksByTask(int stage)
        {
            return new DaemonTask[] { };
        }

        IEnumerable<DaemonTask> IDataPlugin.DaemonTasksBlocked(string buildId, int order)
        {
            return new DaemonTask[] { };
        }

        IEnumerable<DaemonTask> IDataPlugin.DaemonTasksBlockedForJob(string jobid, int order)
        {
            return new DaemonTask[] { };
        }

        IEnumerable<DaemonTask> IDataPlugin.GetBlockingDaemonTasks() 
        {
            return new DaemonTask[] { };
        }

        IEnumerable<DaemonTask> IDataPlugin.GetFailingDaemonTasks()
        {
            return new DaemonTask[] { };
        }

        PageableData<DaemonTask> IDataPlugin.PageDaemonTasks(int index, int pageSize, string orderBy = "", string filterBy = "", string jobId = "") 
        {
            return new PageableData<DaemonTask>(new DaemonTask[] { }, 0, 0, 0);
        }

        IEnumerable<string> IDataPlugin.GetFailingDaemonTasksBuildIds()
        {
            return new string[] { };
        }

        #endregion

        #region MUTATIONREPORT

        MutationReport IDataPlugin.SaveMutationReport(MutationReport incidentSummary)
        {
            return new MutationReport { };
        }

        IEnumerable<MutationReport> IDataPlugin.GetMutationReportsForBuild(string buildId) 
        {
            return new MutationReport[] { };
        }

        MutationReport IDataPlugin.GetMutationReportByBuild(string buildId) 
        {
            return new MutationReport { };
        }

        MutationReport IDataPlugin.GetMutationReportById(string id)
        {
            return new MutationReport { };
        }

        bool IDataPlugin.DeleteMutationReport(MutationReport record)
        {
            return true;
        }

        #endregion
    }
}