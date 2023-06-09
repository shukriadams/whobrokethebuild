using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    public class DataLayerPluginProxy : PluginProxy, IDataLayerPlugin
    {
        private readonly IPluginSender _pluginSender;

        public DataLayerPluginProxy(IPluginSender pluginSender) : base(pluginSender)
        {
            _pluginSender = pluginSender;
        }

        #region UTIL

        public void Diagnose()
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = "Diagnose"
            });
        }

        public string Verify()
        {
            return _pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = "Verify"
            });
        }

        public object InitializeDatastore() 
        {
            return _pluginSender.InvokeMethod<object>(this, new PluginArgs
            {
                FunctionName = "InitializeDatastore"
            });
        }

        #endregion

        #region STOREITEM


        StoreItem IDataLayerPlugin.SaveStore(StoreItem storeItem)
        {
            return _pluginSender.InvokeMethod<StoreItem>(this, new PluginArgs
            {
                FunctionName = "SaveStore",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "storeItem", Value = storeItem }
                }
            });
        }

        StoreItem IDataLayerPlugin.GetStoreItemByItem(string id)
        {
            return _pluginSender.InvokeMethod<StoreItem>(this, new PluginArgs
            {
                FunctionName = "GetStoreItemByItem",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        StoreItem IDataLayerPlugin.GetStoreItemByKey(string key)
        {
            return _pluginSender.InvokeMethod<StoreItem>(this, new PluginArgs
            {
                FunctionName = "GetStoreItemByKey",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "key", Value = key }
                }
            });
        }

        bool IDataLayerPlugin.DeleteStoreItem(StoreItem record)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = "DeleteStoreItem",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "record", Value = record }
                }
            });
        }

        #endregion

        #region BUILD SERVER

        BuildServer IDataLayerPlugin.SaveBuildServer(BuildServer buildServer)
        {
            return _pluginSender.InvokeMethod<BuildServer>(this, new PluginArgs
            {
                FunctionName = "SaveBuildServer",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildServer", Value = buildServer }
                }
            });
        }

        BuildServer IDataLayerPlugin.GetBuildServerById(string id)
        {
            return _pluginSender.InvokeMethod<BuildServer>(this, new PluginArgs
            {
                FunctionName = "GetBuildServerById",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        BuildServer IDataLayerPlugin.GetBuildServerByKey(string key)
        {
            return _pluginSender.InvokeMethod<BuildServer>(this, new PluginArgs
            {
                FunctionName = "GetBuildServerByKey",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "key", Value = key }
                }
            });
        }

        IEnumerable<BuildServer> IDataLayerPlugin.GetBuildServers()
        {
            return _pluginSender.InvokeMethod<IEnumerable<BuildServer>>(this, new PluginArgs
            {
                FunctionName = "GetBuildServers"
            });
        }

        bool IDataLayerPlugin.DeleteBuildServer(BuildServer record)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = "DeleteBuildServer",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "record", Value = record }
                }
            });
        }

        #endregion

        #region SOURCE SERVER

        SourceServer IDataLayerPlugin.SaveSourceServer(SourceServer sourceServer)
        {
            return _pluginSender.InvokeMethod<SourceServer>(this, new PluginArgs
            {
                FunctionName = "SaveSourceServer",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "sourceServer", Value = sourceServer }
                }
            });
        }

        SourceServer IDataLayerPlugin.GetSourceServerById(string id)
        {
            return _pluginSender.InvokeMethod<SourceServer>(this, new PluginArgs
            {
                FunctionName = "GetSourceServerById",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        SourceServer IDataLayerPlugin.GetSourceServerByKey(string key)
        {
            return _pluginSender.InvokeMethod<SourceServer>(this, new PluginArgs
            {
                FunctionName = "GetSourceServerByKey",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "key", Value = key }
                }
            });
        }

        IEnumerable<SourceServer> IDataLayerPlugin.GetSourceServers()
        {
            return _pluginSender.InvokeMethod<IEnumerable<SourceServer>>(this, new PluginArgs
            {
                FunctionName = "GetSourceServers"
            });
        }

        bool IDataLayerPlugin.DeleteSourceServer(SourceServer record)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = "DeleteSourceServer",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "record", Value = record }
                }
            });
        }

        #endregion

        #region JOB

        Job IDataLayerPlugin.SaveJob(Job job)
        {
            return _pluginSender.InvokeMethod<Job>(this, new PluginArgs
            {
                FunctionName = "SaveJob",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }

        Job IDataLayerPlugin.GetJobById(string id)
        {
            return _pluginSender.InvokeMethod<Job>(this, new PluginArgs
            {
                FunctionName = "GetJobById",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        Job IDataLayerPlugin.GetJobByKey(string key)
        {
            return _pluginSender.InvokeMethod<Job>(this, new PluginArgs
            {
                FunctionName = "GetJobByKey",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "key", Value = key }
                }
            });
        }

        IEnumerable<Job> IDataLayerPlugin.GetJobs()
        {
            return _pluginSender.InvokeMethod<IEnumerable<Job>>(this, new PluginArgs
            {
                FunctionName = "GetJobs"
            });
        }

        IEnumerable<Job> IDataLayerPlugin.GetJobsByBuildServerId(string buildServerId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Job>>(this, new PluginArgs
            {
                FunctionName = "GetJobsByBuildServerId",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildServerId", Value = buildServerId }
                }
            });
        }

        bool IDataLayerPlugin.DeleteJob(Job job)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = "DeleteJob",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }

        IEnumerable<string> IDataLayerPlugin.GetIncidentIdsForJob(Job job)
        {
            return _pluginSender.InvokeMethod<IEnumerable<string>>(this, new PluginArgs
            {
                FunctionName = "GetIncidentIdsForJob",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });

        }

        JobStats IDataLayerPlugin.GetJobStats(Job job)
        {
            return _pluginSender.InvokeMethod<JobStats>(this, new PluginArgs
            {
                FunctionName = "GetJobStats",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });

        }

        int IDataLayerPlugin.ResetJob(string jobId, bool hard)
        {
            return _pluginSender.InvokeMethod<int>(this, new PluginArgs
            {
                FunctionName = "ResetJob",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "jobId", Value = jobId },
                    new PluginFunctionParameter { Name = "hard", Value = hard }
                }
            });
        }

        #endregion

        #region USER

        User IDataLayerPlugin.GetUserById(string id)
        {
            return _pluginSender.InvokeMethod<User>(this, new PluginArgs
            {
                FunctionName = "GetUserById",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        User IDataLayerPlugin.GetUserByKey(string key)
        {
            return _pluginSender.InvokeMethod<User>(this, new PluginArgs
            {
                FunctionName = "GetUserByKey",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "key", Value = key }
                }
            });
        }

        User IDataLayerPlugin.SaveUser(User user)
        {
            return _pluginSender.InvokeMethod<User>(this, new PluginArgs
            {
                FunctionName = "SaveUser",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "user", Value = user }
                }
            });
        }

        IEnumerable<User> IDataLayerPlugin.GetUsers()
        {
            return _pluginSender.InvokeMethod<IEnumerable<User>>(this, new PluginArgs
            {
                FunctionName = "GetUsers"
            });
        }

        PageableData<User> IDataLayerPlugin.PageUsers(int index, int pageSize)
        {
            return _pluginSender.InvokeMethod<PageableData<User>>(this, new PluginArgs
            {
                FunctionName = "PageUsers",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "index", Value = index },
                    new PluginFunctionParameter { Name = "pageSize", Value = pageSize }
                }
            });
        }

        bool IDataLayerPlugin.DeleteUser(User record)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = "DeleteUser",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "record", Value = record }
                }
            });
        }

        #endregion

        #region BUILD

        Build IDataLayerPlugin.SaveBuild(Build build)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = "SaveBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        Build IDataLayerPlugin.GetBuildById(string id)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = "GetBuildById",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        Build IDataLayerPlugin.GetBuildByKey(string jobId, string key)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = "GetBuildByKey",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "jobId", Value = jobId },
                    new PluginFunctionParameter { Name = "key", Value = key }
                }
            });
        }

        PageableData<Build> IDataLayerPlugin.PageBuildsByJob(string jobId, int index, int pageSize)
        {
            return _pluginSender.InvokeMethod<PageableData<Build>>(this, new PluginArgs
            {
                FunctionName = "PageBuildsByJob",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "jobId", Value = jobId },
                    new PluginFunctionParameter { Name = "index", Value = index },
                    new PluginFunctionParameter { Name = "pageSize", Value = pageSize }
                }
            });
        }

        PageableData<Build> IDataLayerPlugin.PageIncidentsByJob(string jobId, int index, int pageSize)
        {
            return _pluginSender.InvokeMethod<PageableData<Build>>(this, new PluginArgs
            {
                FunctionName = "PageIncidentsByJob",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "jobId", Value = jobId },
                    new PluginFunctionParameter { Name = "index", Value = index },
                    new PluginFunctionParameter { Name = "pageSize", Value = pageSize }
                }
            });
        }

        IEnumerable<Build> IDataLayerPlugin.GetBuildsByIncident(string incidentId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Build>>(this, new PluginArgs
            {
                FunctionName = "GetBuildsByIncident",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "incidentId", Value = incidentId }
                }
            });
        }

        bool IDataLayerPlugin.DeleteBuild(Build record)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = "DeleteBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "record", Value = record }
                }
            });
        }

        IEnumerable<Build> IDataLayerPlugin.GetBuildsWithNoLog(Job job)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Build>>(this, new PluginArgs
            {
                FunctionName = "GetBuildsWithNoLog",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }

        IEnumerable<Build> IDataLayerPlugin.GetFailingBuildsWithoutIncident(Job job)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Build>>(this, new PluginArgs
            {
                FunctionName = "GetFailingBuildsWithoutIncident",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }

        IEnumerable<Build> IDataLayerPlugin.GetBuildsWithNoInvolvements(Job job)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Build>>(this, new PluginArgs
            {
                FunctionName = "GetBuildsWithNoInvolvements",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }

        Build IDataLayerPlugin.GetLatestBuildByJob(Job job)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = "GetLatestBuildByJob",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }

        Build IDataLayerPlugin.GetDeltaBuildAtBuild(Build build)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = "GetDeltaBuildAtBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        Build IDataLayerPlugin.GetFirstPassingBuildAfterBuild(Build build)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = "GetFirstPassingBuildAfterBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        Build IDataLayerPlugin.GetBreakingBuildByJob(Job job)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = "GetBreakingBuildByJob",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }

        Build IDataLayerPlugin.GetPreviousBuild(Build build)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = "GetPreviousBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        Build IDataLayerPlugin.GetNextBuild(Build build)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = "GetNextBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        IEnumerable<Build> IDataLayerPlugin.GetUnparsedBuildLogs(Job job)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Build>>(this, new PluginArgs
            {
                FunctionName = "GetUnparsedBuildLogs",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }



        PageableData<Build> IDataLayerPlugin.PageBuildsByBuildAgent(string hostname, int index, int pageSize)
        {
            return _pluginSender.InvokeMethod<PageableData<Build>>(this, new PluginArgs
            {
                FunctionName = "PageBuildsByBuildAgent",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "hostname", Value = hostname },
                    new PluginFunctionParameter { Name = "index", Value = index },
                    new PluginFunctionParameter { Name = "pageSize", Value = pageSize }
                }
            });
        }

        int IDataLayerPlugin.ResetBuild(string buildId, bool hard)
        {
            return _pluginSender.InvokeMethod<int>(this, new PluginArgs
            {
                FunctionName = "ResetBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildId", Value = buildId },
                    new PluginFunctionParameter { Name = "hard", Value = hard }
                }
            });
        }

        IEnumerable<Build> IDataLayerPlugin.GetBuildsForPostProcessing(string jobid, string processorKey, int limit)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Build>>(this, new PluginArgs
            {
                FunctionName = "GetBuildsForPostProcessing",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "jobid", Value = jobid },
                    new PluginFunctionParameter { Name = "processorKey", Value = processorKey },
                    new PluginFunctionParameter { Name = "limit", Value = limit }
                }
            });
        }

        #endregion

        #region BUILDFLAG

        BuildFlag IDataLayerPlugin.SaveBuildFlag(BuildFlag flag)
        {
            return _pluginSender.InvokeMethod<BuildFlag>(this, new PluginArgs
            {
                FunctionName = "SaveBuildFlag",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "flag", Value = flag }
                }
            });
        }

        BuildFlag IDataLayerPlugin.GetBuildFlagById(string id)
        {
            return _pluginSender.InvokeMethod<BuildFlag>(this, new PluginArgs
            {
                FunctionName = "GetBuildFlagById",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        bool IDataLayerPlugin.DeleteBuildFlag(BuildFlag buildFlag)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = "DeleteBuildFlag",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildFlag", Value = buildFlag }
                }
            });
        }

        int IDataLayerPlugin.IgnoreBuildFlagsForBuild(Build build, BuildFlags flag)
        {
            return _pluginSender.InvokeMethod<int>(this, new PluginArgs
            {
                FunctionName = "IgnoreBuildFlagsForBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build },
                    new PluginFunctionParameter { Name = "flag", Value = flag }
                }
            });
        }

        int IDataLayerPlugin.DeleteBuildFlagsForBuild(Build build, BuildFlags flag)
        {
            return _pluginSender.InvokeMethod<int>(this, new PluginArgs
            {
                FunctionName = "DeleteBuildFlagsForBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build },
                    new PluginFunctionParameter { Name = "flag", Value = flag }
                }
            });
        }

        IEnumerable<BuildFlag> IDataLayerPlugin.GetBuildFlagsForBuild(Build build)
        {
            return _pluginSender.InvokeMethod<IEnumerable<BuildFlag>>(this, new PluginArgs
            {
                FunctionName = "GetBuildFlagsForBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        PageableData<BuildFlag> IDataLayerPlugin.PageBuildFlags(int index, int pageSize)
        {
            return _pluginSender.InvokeMethod<PageableData<BuildFlag>>(this, new PluginArgs
            {
                FunctionName = "PageBuildFlags",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "index", Value = index },
                    new PluginFunctionParameter { Name = "pageSize", Value = pageSize }
                }
            });
        }

        #endregion

        #region BUILD LOG PARSE RESULT

        BuildLogParseResult IDataLayerPlugin.SaveBuildLogParseResult(BuildLogParseResult buildLog)
        {
            return _pluginSender.InvokeMethod<BuildLogParseResult>(this, new PluginArgs
            {
                FunctionName = "SaveBuildLogParseResult",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildLog", Value = buildLog }
                }
            });
        }

        IEnumerable<BuildLogParseResult> IDataLayerPlugin.GetBuildLogParseResultsByBuildId(string buildId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<BuildLogParseResult>>(this, new PluginArgs
            {
                FunctionName = "GetBuildLogParseResultsByBuildId",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildId", Value = buildId }
                }
            });
        }

        bool IDataLayerPlugin.DeleteBuildLogParseResult(BuildLogParseResult result)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = "DeleteBuildLogParseResult",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "result", Value = result }
                }
            });
        }

        #endregion

        #region BUILD INVOLVEMENT

        BuildInvolvement IDataLayerPlugin.SaveBuildInvolement(BuildInvolvement buildInvolvement)
        {
            return _pluginSender.InvokeMethod<BuildInvolvement>(this, new PluginArgs
            {
                FunctionName = "SaveBuildInvolement",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildInvolvement", Value = buildInvolvement }
                }
            });
        }

        BuildInvolvement IDataLayerPlugin.GetBuildInvolvementById(string id)
        {
            return _pluginSender.InvokeMethod<BuildInvolvement>(this, new PluginArgs
            {
                FunctionName = "GetBuildInvolvementById",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        BuildInvolvement IDataLayerPlugin.GetBuildInvolvementByRevisionCode(string buildid, string revisionCode)
        {
            return _pluginSender.InvokeMethod<BuildInvolvement>(this, new PluginArgs
            {
                FunctionName = "GetBuildInvolvementByRevisionCode",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildid", Value = buildid },
                    new PluginFunctionParameter { Name = "revisionCode", Value = revisionCode }
                }
            });
        }

        bool IDataLayerPlugin.DeleteBuildInvolvement(BuildInvolvement record)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = "DeleteBuildInvolvement",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "record", Value = record }
                }
            });
        }

        IEnumerable<BuildInvolvement> IDataLayerPlugin.GetBuildInvolvementsByBuild(string buildId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<BuildInvolvement>>(this, new PluginArgs
            {
                FunctionName = "GetBuildInvolvementsByBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildId", Value = buildId }
                }
            });
        }

        IEnumerable<BuildInvolvement> IDataLayerPlugin.GetBuildInvolvementsWithoutMappedUser(string jobId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<BuildInvolvement>>(this, new PluginArgs
            {
                FunctionName = "GetBuildInvolvementsWithoutMappedUser",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "jobId", Value = jobId }
                }
            });
        }

        IEnumerable<BuildInvolvement> IDataLayerPlugin.GetBuildInvolvementsWithoutMappedRevisions(string jobId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<BuildInvolvement>>(this, new PluginArgs
            {
                FunctionName = "GetBuildInvolvementsWithoutMappedRevisions",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "jobId", Value = jobId }
                }
            });
        }

        IEnumerable<BuildInvolvement> IDataLayerPlugin.GetBuildInvolvementByUserId(string userId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<BuildInvolvement>>(this, new PluginArgs
            {
                FunctionName = "GetBuildInvolvementByUserId",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "userId", Value = userId }
                }
            });
        }

        PageableData<BuildInvolvement> IDataLayerPlugin.PageBuildInvolvementsByUserAndStatus(string userid, BuildStatus buildStatus, int index, int pageSize)
        {
            return _pluginSender.InvokeMethod<PageableData<BuildInvolvement>>(this, new PluginArgs
            {
                FunctionName = "PageBuildInvolvementsByUserAndStatus",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "userid", Value = userid },
                    new PluginFunctionParameter { Name = "buildStatus", Value = buildStatus },
                    new PluginFunctionParameter { Name = "index", Value = index },
                    new PluginFunctionParameter { Name = "pageSize", Value = pageSize }
                }
            });
        }

        #endregion

        #region BUILD PROCESSOR

        BuildProcessor IDataLayerPlugin.GetBuildProcessorById(string id)
        {
            return _pluginSender.InvokeMethod<BuildProcessor>(this, new PluginArgs
            {
                FunctionName = "GetBuildProcessorById",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }


        BuildProcessor IDataLayerPlugin.SaveBuildProcessor(BuildProcessor buildProcessor)
        {
            return _pluginSender.InvokeMethod<BuildProcessor>(this, new PluginArgs
            {
                FunctionName = "SaveBuildProcessor",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildProcessor", Value = buildProcessor }
                }
            });
        }

        IEnumerable<BuildProcessor> IDataLayerPlugin.GetBuildProcessorsByBuildId(string buildId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<BuildProcessor>>(this, new PluginArgs
            {
                FunctionName = "GetBuildProcessorsByBuildId",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildId", Value = buildId }
                }
            });
        }

        #endregion

        #region REVISION

        Revision IDataLayerPlugin.SaveRevision(Revision revision)
        {
            return _pluginSender.InvokeMethod<Revision>(this, new PluginArgs
            {
                FunctionName = "SaveRevision",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "revision", Value = revision }
                }
            });
        }

        Revision IDataLayerPlugin.GetRevisionById(string id)
        {
            return _pluginSender.InvokeMethod<Revision>(this, new PluginArgs
            {
                FunctionName = "GetRevisionById",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        Revision IDataLayerPlugin.GetRevisionByKey(string key)
        {
            return _pluginSender.InvokeMethod<Revision>(this, new PluginArgs
            {
                FunctionName = "GetRevisionByKey",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "key", Value = key }
                }
            });
        }

        Revision IDataLayerPlugin.GetNewestRevisionForBuild(string buildId)
        {
            return _pluginSender.InvokeMethod<Revision>(this, new PluginArgs
            {
                FunctionName = "GetNewestRevisionForBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildId", Value = buildId }
                }
            });
        }

        IEnumerable<Revision> IDataLayerPlugin.GetRevisionByBuild(string buildId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Revision>>(this, new PluginArgs
            {
                FunctionName = "GetRevisionByBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildId", Value = buildId }
                }
            });
        }

        IEnumerable<Revision> IDataLayerPlugin.GetRevisionsBySourceServer(string sourceServerId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Revision>>(this, new PluginArgs
            {
                FunctionName = "GetRevisionsBySourceServer",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "sourceServerId", Value = sourceServerId }
                }
            });
        }

        bool IDataLayerPlugin.DeleteRevision(Revision revision)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = "DeleteRevision",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "revision", Value = revision }
                }
            });
        }

        #endregion

        #region SESSION

        Session IDataLayerPlugin.SaveSession(Session session)
        {
            return _pluginSender.InvokeMethod<Session>(this, new PluginArgs
            {
                FunctionName = "SaveSession",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "session", Value = session }
                }
            });
        }

        Session IDataLayerPlugin.GetSessionById(string id)
        {
            return _pluginSender.InvokeMethod<Session>(this, new PluginArgs
            {
                FunctionName = "GetSessionById",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        IEnumerable<Session> IDataLayerPlugin.GetSessionByUserId(string userid)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Session>>(this, new PluginArgs
            {
                FunctionName = "GetSessionByUserId",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "userid", Value = userid }
                }
            });
        }

        bool IDataLayerPlugin.DeleteSession(Session session)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = "DeleteSession",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "session", Value = session }
                }
            });
        }

        #endregion

        #region JOB DELTA

        Build IDataLayerPlugin.GetLastJobDelta(string jobId)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = "GetLastJobDelta",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "jobId", Value = jobId }
                }
            });
        }

        void IDataLayerPlugin.SaveJobDelta(Build build)
        {
            _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = "SaveJobDelta",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        #endregion

        #region CONFIGURATIONSETTING

        ConfigurationState IDataLayerPlugin.AddConfigurationState(ConfigurationState configurationState)
        {
            return _pluginSender.InvokeMethod<ConfigurationState>(this, new PluginArgs
            {
                FunctionName = "AddConfigurationState",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "configurationState", Value = configurationState }
                }
            });
        }

        public int ClearAllTables() 
        {
            return _pluginSender.InvokeMethod<int>(this, new PluginArgs
            {
                FunctionName = "ClearAllTables"
            });
        }

        public ConfigurationState GetLatestConfigurationState()
        {
            return _pluginSender.InvokeMethod<ConfigurationState>(this, new PluginArgs
            {
                FunctionName = "GetLatestConfigurationState"
            });
        }

        public PageableData<ConfigurationState> PageConfigurationStates(int index, int pageSize)
        {
            return _pluginSender.InvokeMethod<PageableData<ConfigurationState>>(this, new PluginArgs
            {
                FunctionName = "PageConfigurationStates",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "index", Value = index },
                    new PluginFunctionParameter { Name = "pageSize", Value = pageSize }
                }
            });
        }

        #endregion
    }
}
