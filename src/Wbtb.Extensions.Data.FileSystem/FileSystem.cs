using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.FileSystem
{
    internal class FileSystem : Plugin, IDataLayerPlugin
    {
        public ConfigurationState AddConfigurationState(ConfigurationState configurationState)
        {
            return null;
        }

        public ReachAttemptResult AttemptReach()
        {
            return new ReachAttemptResult {  Reachable = true };
        }

        public bool DeleteBuild(Build record)
        {
            return false;
        }

        public bool DeleteBuildFlag(BuildFlag record)
        {
            return false;
        }

        public int DeleteBuildFlagsForBuild(Build build, BuildFlags flag)
        {
            return 0;
        }

        public bool DeleteBuildInvolvement(BuildInvolvement record)
        {
            return false;
        }

        public bool DeleteBuildLogParseResult(BuildLogParseResult record)
        {
            return false;
        }

        public bool DeleteBuildServer(BuildServer record)
        {
            return false;
        }

        public bool DeleteJob(Job record)
        {
            return false;
        }

        public bool DeleteRevision(Revision record)
        {
            return false;
        }

        public bool DeleteSession(Session record)
        {
            return false;
        }

        public bool DeleteSourceServer(SourceServer record)
        {
            return false;
        }

        public bool DeleteStoreItem(StoreItem record)
        {
            return false;
        }

        public bool DeleteUser(User record)
        {
            return false;
        }

        public Build GetBreakingBuildByJob(Job job)
        {
            return null;
        }

        public Build GetBuildById(string id)
        {
            return new Build { };
        }

        public Build GetBuildByKey(string jobId, string key)
        {
            return new Build { };
        }

        public BuildFlag GetBuildFlagById(string id)
        {
            return new BuildFlag { };
        }

        public IEnumerable<BuildFlag> GetBuildFlagsForBuild(Build build)
        {
            return new BuildFlag[] { };
        }

        public BuildInvolvement GetBuildInvolvementById(string id)
        {
            return new BuildInvolvement { };
        }

        public BuildInvolvement GetBuildInvolvementByRevisionCode(string buildid, string revisionCode)
        {
            return new BuildInvolvement { };
        }

        public IEnumerable<BuildInvolvement> GetBuildInvolvementByUserId(string userId)
        {
            return new BuildInvolvement[] { };
        }

        public IEnumerable<BuildInvolvement> GetBuildInvolvementsByBuild(string buildId)
        {
            return new BuildInvolvement[] { };
        }

        public IEnumerable<BuildInvolvement> GetBuildInvolvementsWithoutMappedRevisions(string jobId)
        {
            return new BuildInvolvement[] { };
        }

        public IEnumerable<BuildInvolvement> GetBuildInvolvementsWithoutMappedUser(string jobId)
        {
            return new BuildInvolvement[] { };
        }

        public IEnumerable<BuildLogParseResult> GetBuildLogParseResultsByBuildId(string buildId)
        {
            return new BuildLogParseResult[] { };
        }

        public BuildProcessor GetBuildProcessorById(string id)
        {
            return new BuildProcessor { };
        }

        public IEnumerable<BuildProcessor> GetBuildProcessorsByBuildId(string buildId)
        {
            return new BuildProcessor[] { };
        }

        public BuildServer GetBuildServerById(string id)
        {
            return new BuildServer { };
        }

        public BuildServer GetBuildServerByKey(string key)
        {
            return new BuildServer { };
        }

        public IEnumerable<BuildServer> GetBuildServers()
        {
            return new BuildServer[] { };
        }

        public IEnumerable<Build> GetBuildsForPostProcessing(string jobid, string processorKey, int limit)
        {
            return new Build[] { };
        }

        public IEnumerable<Build> GetBuildsWithNoInvolvements(Job job)
        {
            return new Build[] { };
        }

        public IEnumerable<Build> GetBuildsWithNoLog(Job job)
        {
            return new Build[] { };
        }

        public IEnumerable<Build> GetFailingBuildsWithoutIncident(Job job)
        {
            return new Build[] { };
        }

        public Build GetFirstPassingBuildAfterBuild(Build build)
        {
            return new Build { };
        }

        public Build GetDeltaBuildAtBuild(Build build) 
        {
            return new Build { };
        }

        public IEnumerable<string> GetIncidentIdsForJob(Job job)
        {
            return new string[] { };
        }

        public Job GetJobById(string id)
        {
            return new Job { };
        }

        public Job GetJobByKey(string key)
        {
            return new Job { };
        }

        public IEnumerable<Job> GetJobs()
        {
            return new Job[] { };
        }

        public IEnumerable<Job> GetJobsByBuildServerId(string buildServerId)
        {
            return new Job[] { };
        }

        public JobStats GetJobStats(Job job)
        {
            return new JobStats { };
        }

        public Build GetLastJobDelta(string jobId)
        {
            return new Build { };
        }

        public Build GetLatestBuildByJob(Job job)
        {
            return new Build { };
        }

        public ConfigurationState GetLatestConfigurationState()
        {
            return new ConfigurationState { };
        }

        public Revision GetNewestRevisionForBuild(string buildId)
        {
            return new Revision { };
        }

        public Build GetNextBuild(Build build)
        {
            return new Build { };
        }

        public Build GetPreviousBuild(Build build)
        {
            return new Build { };
        }

        public IEnumerable<Revision> GetRevisionByBuild(string buildId)
        {
            return new Revision[] { };
        }

        public Revision GetRevisionById(string id)
        {
            return new Revision { };
        }

        public Revision GetRevisionByKey(string key)
        {
            return new Revision { };
        }

        public IEnumerable<Revision> GetRevisionsBySourceServer(string sourceServerId)
        {
            return new Revision[] { };
        }

        public Session GetSessionById(string id)
        {
            return new Session { };
        }

        public IEnumerable<Session> GetSessionByUserId(string userid)
        {
            return new Session[] { };
        }

        public SourceServer GetSourceServerById(string id)
        {
            return new SourceServer { };
        }

        public SourceServer GetSourceServerByKey(string key)
        {
            return new SourceServer { };
        }

        public IEnumerable<SourceServer> GetSourceServers()
        {
            return new SourceServer[] { };
        }

        public StoreItem GetStoreItemByItem(string id)
        {
            return new StoreItem { };
        }

        public StoreItem GetStoreItemByKey(string key)
        {
            return new StoreItem { };
        }

        public IEnumerable<Build> GetUnparsedBuildLogs(Job job)
        {
            return new Build[] { };
        }

        public User GetUserById(string id)
        {
            return new User { };
        }

        public User GetUserByKey(string key)
        {
            return new User { };
        }

        public IEnumerable<User> GetUsers()
        {
            return new User[] { };
        }

        public int IgnoreBuildFlagsForBuild(Build build, BuildFlags flag)
        {
            return 0;
        }

        public object InitializeDatastore()
        {
            return null;
        }

        public PluginInitResult InitializePlugin()
        {
            return new PluginInitResult { Success = true };
        }

        public PageableData<BuildFlag> PageBuildFlags(int index, int pageSize)
        {
            return new PageableData<BuildFlag>(new BuildFlag[] { }, 0, 0, 0);
        }

        public PageableData<BuildInvolvement> PageBuildInvolvementsByUserAndStatus(string userid, BuildStatus buildStatus, int index, int pageSize)
        {
            return new PageableData<BuildInvolvement>(new BuildInvolvement[] { }, 0, 0, 0);
        }

        public PageableData<Build> PageBuildsByBuildAgent(string hostname, int index, int pageSize)
        {
            return new PageableData<Build>(new Build[] { }, 0, 0, 0);
        }

        public IEnumerable<Build> GetBuildsByIncident(string incidentId)
        {
            return new Build[] { };
        }

        public PageableData<Build> PageBuildsByJob(string jobId, int index, int pageSize)
        {
            return new PageableData<Build>(new Build[] { }, 0, 0, 0);
        }

        public PageableData<ConfigurationState> PageConfigurationStates(int index, int pageSize)
        {
            return new PageableData<ConfigurationState>(new ConfigurationState[] { }, 0, 0, 0);
        }

        public PageableData<Build> PageIncidentsByJob(string jobId, int index, int pageSize)
        {
            return new PageableData<Build>(new Build[] { }, 0, 0, 0);
        }

        public PageableData<User> PageUsers(int index, int pageSize)
        {
            return new PageableData<User>(new User[] { }, 0, 0, 0);
        }

        public int ResetBuild(string buildId, bool hard)
        {
            return 0;
        }

        public int ResetJob(string jobId, bool hard)
        {
            return 0;
        }

        public Build SaveBuild(Build build)
        {
            return new Build { };
        }

        public BuildFlag SaveBuildFlag(BuildFlag flag)
        {
            return new BuildFlag { };
        }

        public BuildInvolvement SaveBuildInvolement(BuildInvolvement buildInvolvement)
        {
            return new BuildInvolvement { };
        }

        public BuildLogParseResult SaveBuildLogParseResult(BuildLogParseResult buildLog)
        {
            return new BuildLogParseResult { };
        }

        public BuildProcessor SaveBuildProcessor(BuildProcessor buildProcessor)
        {
            return new BuildProcessor { };
        }

        public BuildServer SaveBuildServer(BuildServer buildServer)
        {
            return new BuildServer { };
        }

        public Job SaveJob(Job job)
        {
            return new Job { };
        }

        public void SaveJobDelta(Build build)
        {
            
        }

        public Revision SaveRevision(Revision revision)
        {
            return new Revision { };
        }

        public Session SaveSession(Session session)
        {
            return new Session { };
        }

        public SourceServer SaveSourceServer(SourceServer sourceServer)
        {
            return new SourceServer { };
        }

        public StoreItem SaveStore(StoreItem storeItem)
        {
            return new StoreItem { };
        }

        public User SaveUser(User user)
        {
            return new User { };
        }

        public int ClearAllTables() 
        {
            return 0;
        }
    }
}
