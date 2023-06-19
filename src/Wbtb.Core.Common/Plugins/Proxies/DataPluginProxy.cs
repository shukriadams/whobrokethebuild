using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    public class DataPluginProxy : PluginProxy, IDataPlugin
    {
        private readonly IPluginSender _pluginSender;

        public DataPluginProxy(IPluginSender pluginSender) : base(pluginSender)
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


        StoreItem IDataPlugin.SaveStore(StoreItem storeItem)
        {
            return _pluginSender.InvokeMethod<StoreItem>(this, new PluginArgs
            {
                FunctionName = "SaveStore",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "storeItem", Value = storeItem }
                }
            });
        }

        StoreItem IDataPlugin.GetStoreItemByItem(string id)
        {
            return _pluginSender.InvokeMethod<StoreItem>(this, new PluginArgs
            {
                FunctionName = "GetStoreItemByItem",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        StoreItem IDataPlugin.GetStoreItemByKey(string key)
        {
            return _pluginSender.InvokeMethod<StoreItem>(this, new PluginArgs
            {
                FunctionName = "GetStoreItemByKey",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "key", Value = key }
                }
            });
        }

        bool IDataPlugin.DeleteStoreItem(StoreItem record)
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

        BuildServer IDataPlugin.SaveBuildServer(BuildServer buildServer)
        {
            return _pluginSender.InvokeMethod<BuildServer>(this, new PluginArgs
            {
                FunctionName = "SaveBuildServer",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildServer", Value = buildServer }
                }
            });
        }

        BuildServer IDataPlugin.GetBuildServerById(string id)
        {
            return _pluginSender.InvokeMethod<BuildServer>(this, new PluginArgs
            {
                FunctionName = "GetBuildServerById",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        BuildServer IDataPlugin.GetBuildServerByKey(string key)
        {
            return _pluginSender.InvokeMethod<BuildServer>(this, new PluginArgs
            {
                FunctionName = "GetBuildServerByKey",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "key", Value = key }
                }
            });
        }

        IEnumerable<BuildServer> IDataPlugin.GetBuildServers()
        {
            return _pluginSender.InvokeMethod<IEnumerable<BuildServer>>(this, new PluginArgs
            {
                FunctionName = "GetBuildServers"
            });
        }

        bool IDataPlugin.DeleteBuildServer(BuildServer record)
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

        SourceServer IDataPlugin.SaveSourceServer(SourceServer sourceServer)
        {
            return _pluginSender.InvokeMethod<SourceServer>(this, new PluginArgs
            {
                FunctionName = "SaveSourceServer",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "sourceServer", Value = sourceServer }
                }
            });
        }

        SourceServer IDataPlugin.GetSourceServerById(string id)
        {
            return _pluginSender.InvokeMethod<SourceServer>(this, new PluginArgs
            {
                FunctionName = "GetSourceServerById",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        SourceServer IDataPlugin.GetSourceServerByKey(string key)
        {
            return _pluginSender.InvokeMethod<SourceServer>(this, new PluginArgs
            {
                FunctionName = "GetSourceServerByKey",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "key", Value = key }
                }
            });
        }

        IEnumerable<SourceServer> IDataPlugin.GetSourceServers()
        {
            return _pluginSender.InvokeMethod<IEnumerable<SourceServer>>(this, new PluginArgs
            {
                FunctionName = "GetSourceServers"
            });
        }

        bool IDataPlugin.DeleteSourceServer(SourceServer record)
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

        Job IDataPlugin.SaveJob(Job job)
        {
            return _pluginSender.InvokeMethod<Job>(this, new PluginArgs
            {
                FunctionName = "SaveJob",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }

        Job IDataPlugin.GetJobById(string id)
        {
            return _pluginSender.InvokeMethod<Job>(this, new PluginArgs
            {
                FunctionName = "GetJobById",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        Job IDataPlugin.GetJobByKey(string key)
        {
            return _pluginSender.InvokeMethod<Job>(this, new PluginArgs
            {
                FunctionName = "GetJobByKey",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "key", Value = key }
                }
            });
        }

        IEnumerable<Job> IDataPlugin.GetJobs()
        {
            return _pluginSender.InvokeMethod<IEnumerable<Job>>(this, new PluginArgs
            {
                FunctionName = "GetJobs"
            });
        }

        IEnumerable<Job> IDataPlugin.GetJobsByBuildServerId(string buildServerId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Job>>(this, new PluginArgs
            {
                FunctionName = "GetJobsByBuildServerId",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildServerId", Value = buildServerId }
                }
            });
        }

        bool IDataPlugin.DeleteJob(Job job)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = "DeleteJob",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }

        IEnumerable<string> IDataPlugin.GetIncidentIdsForJob(Job job)
        {
            return _pluginSender.InvokeMethod<IEnumerable<string>>(this, new PluginArgs
            {
                FunctionName = "GetIncidentIdsForJob",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });

        }

        JobStats IDataPlugin.GetJobStats(Job job)
        {
            return _pluginSender.InvokeMethod<JobStats>(this, new PluginArgs
            {
                FunctionName = "GetJobStats",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });

        }

        int IDataPlugin.ResetJob(string jobId, bool hard)
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

        User IDataPlugin.GetUserById(string id)
        {
            return _pluginSender.InvokeMethod<User>(this, new PluginArgs
            {
                FunctionName = "GetUserById",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        User IDataPlugin.GetUserByKey(string key)
        {
            return _pluginSender.InvokeMethod<User>(this, new PluginArgs
            {
                FunctionName = "GetUserByKey",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "key", Value = key }
                }
            });
        }

        User IDataPlugin.SaveUser(User user)
        {
            return _pluginSender.InvokeMethod<User>(this, new PluginArgs
            {
                FunctionName = "SaveUser",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "user", Value = user }
                }
            });
        }

        IEnumerable<User> IDataPlugin.GetUsers()
        {
            return _pluginSender.InvokeMethod<IEnumerable<User>>(this, new PluginArgs
            {
                FunctionName = "GetUsers"
            });
        }

        PageableData<User> IDataPlugin.PageUsers(int index, int pageSize)
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

        bool IDataPlugin.DeleteUser(User record)
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

        Build IDataPlugin.SaveBuild(Build build)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = "SaveBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        Build IDataPlugin.GetBuildById(string id)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = "GetBuildById",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        Build IDataPlugin.GetBuildByKey(string jobId, string key)
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

        PageableData<Build> IDataPlugin.PageBuildsByJob(string jobId, int index, int pageSize)
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

        PageableData<Build> IDataPlugin.PageIncidentsByJob(string jobId, int index, int pageSize)
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

        IEnumerable<Build> IDataPlugin.GetBuildsByIncident(string incidentId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Build>>(this, new PluginArgs
            {
                FunctionName = "GetBuildsByIncident",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "incidentId", Value = incidentId }
                }
            });
        }

        bool IDataPlugin.DeleteBuild(Build record)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = "DeleteBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "record", Value = record }
                }
            });
        }

        IEnumerable<Build> IDataPlugin.GetBuildsWithNoLog(Job job)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Build>>(this, new PluginArgs
            {
                FunctionName = "GetBuildsWithNoLog",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }

        IEnumerable<Build> IDataPlugin.GetFailingBuildsWithoutIncident(Job job)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Build>>(this, new PluginArgs
            {
                FunctionName = "GetFailingBuildsWithoutIncident",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }

        IEnumerable<Build> IDataPlugin.GetBuildsWithNoInvolvements(Job job)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Build>>(this, new PluginArgs
            {
                FunctionName = "GetBuildsWithNoInvolvements",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }

        Build IDataPlugin.GetLatestBuildByJob(Job job)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = "GetLatestBuildByJob",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }

        Build IDataPlugin.GetDeltaBuildAtBuild(Build build)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = "GetDeltaBuildAtBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        Build IDataPlugin.GetFirstPassingBuildAfterBuild(Build build)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = "GetFirstPassingBuildAfterBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        Build IDataPlugin.GetPreviousBuild(Build build)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = "GetPreviousBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        Build IDataPlugin.GetNextBuild(Build build)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = "GetNextBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        IEnumerable<Build> IDataPlugin.GetUnparsedBuildLogs(Job job)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Build>>(this, new PluginArgs
            {
                FunctionName = "GetUnparsedBuildLogs",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }



        PageableData<Build> IDataPlugin.PageBuildsByBuildAgent(string hostname, int index, int pageSize)
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

        int IDataPlugin.ResetBuild(string buildId, bool hard)
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

        IEnumerable<Build> IDataPlugin.GetBuildsForPostProcessing(string jobid, string processorKey, int limit)
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

        BuildFlag IDataPlugin.SaveBuildFlag(BuildFlag flag)
        {
            return _pluginSender.InvokeMethod<BuildFlag>(this, new PluginArgs
            {
                FunctionName = "SaveBuildFlag",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "flag", Value = flag }
                }
            });
        }

        BuildFlag IDataPlugin.GetBuildFlagById(string id)
        {
            return _pluginSender.InvokeMethod<BuildFlag>(this, new PluginArgs
            {
                FunctionName = "GetBuildFlagById",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        bool IDataPlugin.DeleteBuildFlag(BuildFlag buildFlag)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = "DeleteBuildFlag",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildFlag", Value = buildFlag }
                }
            });
        }

        int IDataPlugin.IgnoreBuildFlagsForBuild(Build build, BuildFlags flag)
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

        int IDataPlugin.DeleteBuildFlagsForBuild(Build build, BuildFlags flag)
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

        IEnumerable<BuildFlag> IDataPlugin.GetBuildFlagsForBuild(Build build)
        {
            return _pluginSender.InvokeMethod<IEnumerable<BuildFlag>>(this, new PluginArgs
            {
                FunctionName = "GetBuildFlagsForBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        PageableData<BuildFlag> IDataPlugin.PageBuildFlags(int index, int pageSize)
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

        BuildLogParseResult IDataPlugin.SaveBuildLogParseResult(BuildLogParseResult buildLog)
        {
            return _pluginSender.InvokeMethod<BuildLogParseResult>(this, new PluginArgs
            {
                FunctionName = "SaveBuildLogParseResult",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildLog", Value = buildLog }
                }
            });
        }

        IEnumerable<BuildLogParseResult> IDataPlugin.GetBuildLogParseResultsByBuildId(string buildId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<BuildLogParseResult>>(this, new PluginArgs
            {
                FunctionName = "GetBuildLogParseResultsByBuildId",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildId", Value = buildId }
                }
            });
        }

        bool IDataPlugin.DeleteBuildLogParseResult(BuildLogParseResult result)
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

        BuildInvolvement IDataPlugin.SaveBuildInvolement(BuildInvolvement buildInvolvement)
        {
            return _pluginSender.InvokeMethod<BuildInvolvement>(this, new PluginArgs
            {
                FunctionName = "SaveBuildInvolement",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildInvolvement", Value = buildInvolvement }
                }
            });
        }

        BuildInvolvement IDataPlugin.GetBuildInvolvementById(string id)
        {
            return _pluginSender.InvokeMethod<BuildInvolvement>(this, new PluginArgs
            {
                FunctionName = "GetBuildInvolvementById",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        BuildInvolvement IDataPlugin.GetBuildInvolvementByRevisionCode(string buildid, string revisionCode)
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

        bool IDataPlugin.DeleteBuildInvolvement(BuildInvolvement record)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = "DeleteBuildInvolvement",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "record", Value = record }
                }
            });
        }

        IEnumerable<BuildInvolvement> IDataPlugin.GetBuildInvolvementsByBuild(string buildId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<BuildInvolvement>>(this, new PluginArgs
            {
                FunctionName = "GetBuildInvolvementsByBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildId", Value = buildId }
                }
            });
        }

        IEnumerable<BuildInvolvement> IDataPlugin.GetBuildInvolvementsWithoutMappedUser(string jobId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<BuildInvolvement>>(this, new PluginArgs
            {
                FunctionName = "GetBuildInvolvementsWithoutMappedUser",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "jobId", Value = jobId }
                }
            });
        }

        IEnumerable<BuildInvolvement> IDataPlugin.GetBuildInvolvementsWithoutMappedRevisions(string jobId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<BuildInvolvement>>(this, new PluginArgs
            {
                FunctionName = "GetBuildInvolvementsWithoutMappedRevisions",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "jobId", Value = jobId }
                }
            });
        }

        IEnumerable<BuildInvolvement> IDataPlugin.GetBuildInvolvementByUserId(string userId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<BuildInvolvement>>(this, new PluginArgs
            {
                FunctionName = "GetBuildInvolvementByUserId",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "userId", Value = userId }
                }
            });
        }

        PageableData<BuildInvolvement> IDataPlugin.PageBuildInvolvementsByUserAndStatus(string userid, BuildStatus buildStatus, int index, int pageSize)
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

        BuildProcessor IDataPlugin.GetBuildProcessorById(string id)
        {
            return _pluginSender.InvokeMethod<BuildProcessor>(this, new PluginArgs
            {
                FunctionName = "GetBuildProcessorById",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }


        BuildProcessor IDataPlugin.SaveBuildProcessor(BuildProcessor buildProcessor)
        {
            return _pluginSender.InvokeMethod<BuildProcessor>(this, new PluginArgs
            {
                FunctionName = "SaveBuildProcessor",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildProcessor", Value = buildProcessor }
                }
            });
        }

        IEnumerable<BuildProcessor> IDataPlugin.GetBuildProcessorsByBuildId(string buildId)
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

        Revision IDataPlugin.SaveRevision(Revision revision)
        {
            return _pluginSender.InvokeMethod<Revision>(this, new PluginArgs
            {
                FunctionName = "SaveRevision",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "revision", Value = revision }
                }
            });
        }

        Revision IDataPlugin.GetRevisionById(string id)
        {
            return _pluginSender.InvokeMethod<Revision>(this, new PluginArgs
            {
                FunctionName = "GetRevisionById",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        Revision IDataPlugin.GetRevisionByKey(string key)
        {
            return _pluginSender.InvokeMethod<Revision>(this, new PluginArgs
            {
                FunctionName = "GetRevisionByKey",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "key", Value = key }
                }
            });
        }

        Revision IDataPlugin.GetNewestRevisionForBuild(string buildId)
        {
            return _pluginSender.InvokeMethod<Revision>(this, new PluginArgs
            {
                FunctionName = "GetNewestRevisionForBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildId", Value = buildId }
                }
            });
        }

        IEnumerable<Revision> IDataPlugin.GetRevisionByBuild(string buildId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Revision>>(this, new PluginArgs
            {
                FunctionName = "GetRevisionByBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildId", Value = buildId }
                }
            });
        }

        IEnumerable<Revision> IDataPlugin.GetRevisionsBySourceServer(string sourceServerId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Revision>>(this, new PluginArgs
            {
                FunctionName = "GetRevisionsBySourceServer",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "sourceServerId", Value = sourceServerId }
                }
            });
        }

        bool IDataPlugin.DeleteRevision(Revision revision)
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

        Session IDataPlugin.SaveSession(Session session)
        {
            return _pluginSender.InvokeMethod<Session>(this, new PluginArgs
            {
                FunctionName = "SaveSession",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "session", Value = session }
                }
            });
        }

        Session IDataPlugin.GetSessionById(string id)
        {
            return _pluginSender.InvokeMethod<Session>(this, new PluginArgs
            {
                FunctionName = "GetSessionById",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        IEnumerable<Session> IDataPlugin.GetSessionByUserId(string userid)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Session>>(this, new PluginArgs
            {
                FunctionName = "GetSessionByUserId",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "userid", Value = userid }
                }
            });
        }

        bool IDataPlugin.DeleteSession(Session session)
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

        Build IDataPlugin.GetLastJobDelta(string jobId)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = "GetLastJobDelta",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "jobId", Value = jobId }
                }
            });
        }

        void IDataPlugin.SaveJobDelta(Build build)
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

        ConfigurationState IDataPlugin.AddConfigurationState(ConfigurationState configurationState)
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
