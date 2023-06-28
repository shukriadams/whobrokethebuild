using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    public class DataPluginProxy : PluginProxy, IDataPlugin
    {
        #region FIELDS

        private readonly IPluginSender _pluginSender;

        #endregion

        #region CTORS

        public DataPluginProxy(IPluginSender pluginSender) : base(pluginSender)
        {
            _pluginSender = pluginSender;
        }

        #endregion

        #region UTIL

        void IPlugin.Diagnose()
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = nameof(IPlugin.Diagnose)
            });
        }

        int IDataPlugin.InitializeDatastore() 
        {
            return _pluginSender.InvokeMethod<int>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.InitializeDatastore)
            });
        }

        int IDataPlugin.DestroyDatastore()
        {
            return _pluginSender.InvokeMethod<int>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.DestroyDatastore)
            });
        }

        #endregion

        #region STOREITEM


        StoreItem IDataPlugin.SaveStore(StoreItem storeItem)
        {
            return _pluginSender.InvokeMethod<StoreItem>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.SaveStore),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "storeItem", Value = storeItem }
                }
            });
        }

        StoreItem IDataPlugin.GetStoreItemByItem(string id)
        {
            return _pluginSender.InvokeMethod<StoreItem>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetStoreItemByItem),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        StoreItem IDataPlugin.GetStoreItemByKey(string key)
        {
            return _pluginSender.InvokeMethod<StoreItem>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetStoreItemByKey),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "key", Value = key }
                }
            });
        }

        bool IDataPlugin.DeleteStoreItem(StoreItem record)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.DeleteStoreItem),
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
                FunctionName = nameof(IDataPlugin.SaveBuildServer),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildServer", Value = buildServer }
                }
            });
        }

        BuildServer IDataPlugin.GetBuildServerById(string id)
        {
            return _pluginSender.InvokeMethod<BuildServer>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetBuildServerById),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        BuildServer IDataPlugin.GetBuildServerByKey(string key)
        {
            return _pluginSender.InvokeMethod<BuildServer>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetBuildServerByKey),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "key", Value = key }
                }
            });
        }

        IEnumerable<BuildServer> IDataPlugin.GetBuildServers()
        {
            return _pluginSender.InvokeMethod<IEnumerable<BuildServer>>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetBuildServers)
            });
        }

        bool IDataPlugin.DeleteBuildServer(BuildServer record)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.DeleteBuildServer),
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
                FunctionName = nameof(IDataPlugin.SaveSourceServer),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "sourceServer", Value = sourceServer }
                }
            });
        }

        SourceServer IDataPlugin.GetSourceServerById(string id)
        {
            return _pluginSender.InvokeMethod<SourceServer>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetSourceServerById),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        SourceServer IDataPlugin.GetSourceServerByKey(string key)
        {
            return _pluginSender.InvokeMethod<SourceServer>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetSourceServerByKey),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "key", Value = key }
                }
            });
        }

        IEnumerable<SourceServer> IDataPlugin.GetSourceServers()
        {
            return _pluginSender.InvokeMethod<IEnumerable<SourceServer>>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetSourceServers)
            });
        }

        bool IDataPlugin.DeleteSourceServer(SourceServer record)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.DeleteSourceServer),
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
                FunctionName = nameof(IDataPlugin.SaveJob),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }

        Job IDataPlugin.GetJobById(string id)
        {
            return _pluginSender.InvokeMethod<Job>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetJobById),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        Job IDataPlugin.GetJobByKey(string key)
        {
            return _pluginSender.InvokeMethod<Job>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetJobByKey),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "key", Value = key }
                }
            });
        }

        IEnumerable<Job> IDataPlugin.GetJobs()
        {
            return _pluginSender.InvokeMethod<IEnumerable<Job>>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetJobs)
            });
        }

        IEnumerable<Job> IDataPlugin.GetJobsByBuildServerId(string buildServerId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Job>>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetJobsByBuildServerId),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildServerId", Value = buildServerId }
                }
            });
        }

        bool IDataPlugin.DeleteJob(Job job)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.DeleteJob),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }

        IEnumerable<string> IDataPlugin.GetIncidentIdsForJob(Job job)
        {
            return _pluginSender.InvokeMethod<IEnumerable<string>>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetIncidentIdsForJob),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });

        }

        JobStats IDataPlugin.GetJobStats(Job job)
        {
            return _pluginSender.InvokeMethod<JobStats>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetJobStats),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });

        }

        int IDataPlugin.ResetJob(string jobId, bool hard)
        {
            return _pluginSender.InvokeMethod<int>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.ResetJob),
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
                FunctionName = nameof(IDataPlugin.GetUserById),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        User IDataPlugin.GetUserByKey(string key)
        {
            return _pluginSender.InvokeMethod<User>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetUserByKey),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "key", Value = key }
                }
            });
        }

        User IDataPlugin.SaveUser(User user)
        {
            return _pluginSender.InvokeMethod<User>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.SaveUser),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "user", Value = user }
                }
            });
        }

        IEnumerable<User> IDataPlugin.GetUsers()
        {
            return _pluginSender.InvokeMethod<IEnumerable<User>>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetUsers)
            });
        }

        PageableData<User> IDataPlugin.PageUsers(int index, int pageSize)
        {
            return _pluginSender.InvokeMethod<PageableData<User>>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.PageUsers),
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
                FunctionName = nameof(IDataPlugin.DeleteUser),
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
                FunctionName = nameof(IDataPlugin.SaveBuild),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        Build IDataPlugin.GetBuildById(string id)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetBuildById),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        Build IDataPlugin.GetBuildByKey(string jobId, string key)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetBuildByKey),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "jobId", Value = jobId },
                    new PluginFunctionParameter { Name = "key", Value = key }
                }
            });
        }

        PageableData<Build> IDataPlugin.PageBuildsByJob(string jobId, int index, int pageSize, bool sortAscending)
        {
            return _pluginSender.InvokeMethod<PageableData<Build>>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.PageBuildsByJob),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "jobId", Value = jobId },
                    new PluginFunctionParameter { Name = "index", Value = index },
                    new PluginFunctionParameter { Name = "pageSize", Value = pageSize },
                    new PluginFunctionParameter { Name = "sortAscending", Value = sortAscending }
                }
            });
        }

        PageableData<Build> IDataPlugin.PageIncidentsByJob(string jobId, int index, int pageSize)
        {
            return _pluginSender.InvokeMethod<PageableData<Build>>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.PageIncidentsByJob),
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
                FunctionName = nameof(IDataPlugin.GetBuildsByIncident),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "incidentId", Value = incidentId }
                }
            });
        }

        bool IDataPlugin.DeleteBuild(Build record)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.DeleteBuild),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "record", Value = record }
                }
            });
        }

        Build IDataPlugin.GetLatestBuildByJob(Job job)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetLatestBuildByJob),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }

        Build IDataPlugin.GetDeltaBuildAtBuild(Build build)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetDeltaBuildAtBuild),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        Build IDataPlugin.GetFirstPassingBuildAfterBuild(Build build)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetFirstPassingBuildAfterBuild),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        Build IDataPlugin.GetPreviousBuild(Build build)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetPreviousBuild),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        Build IDataPlugin.GetNextBuild(Build build)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetNextBuild),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        PageableData<Build> IDataPlugin.PageBuildsByBuildAgent(string hostname, int index, int pageSize)
        {
            return _pluginSender.InvokeMethod<PageableData<Build>>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.PageBuildsByBuildAgent),
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
                FunctionName = nameof(IDataPlugin.ResetBuild),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildId", Value = buildId },
                    new PluginFunctionParameter { Name = "hard", Value = hard }
                }
            });
        }

        #endregion

        #region BUILD LOG PARSE RESULT

        BuildLogParseResult IDataPlugin.SaveBuildLogParseResult(BuildLogParseResult buildLog)
        {
            return _pluginSender.InvokeMethod<BuildLogParseResult>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.SaveBuildLogParseResult),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildLog", Value = buildLog }
                }
            });
        }

        IEnumerable<BuildLogParseResult> IDataPlugin.GetBuildLogParseResultsByBuildId(string buildId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<BuildLogParseResult>>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetBuildLogParseResultsByBuildId),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildId", Value = buildId }
                }
            });
        }

        bool IDataPlugin.DeleteBuildLogParseResult(BuildLogParseResult result)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.DeleteBuildLogParseResult),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "result", Value = result }
                }
            });
        }

        #endregion

        #region R_BuildLogParseResult_BuildInvolvement

        string IDataPlugin.ConnectBuildLogParseResultAndBuildBuildInvolvement(string buildLogParseResultId, string buildInvolvementId)
        {
            return _pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.ConnectBuildLogParseResultAndBuildBuildInvolvement),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildLogParseResultId", Value = buildLogParseResultId },
                    new PluginFunctionParameter { Name = "buildInvolvementId", Value = buildInvolvementId }
                }
            });
        }

        bool IDataPlugin.SplitBuildLogParseResultAndBuildBuildInvolvement(string id)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.SplitBuildLogParseResultAndBuildBuildInvolvement),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        IEnumerable<string> IDataPlugin.GetBuildLogParseResultsForBuildInvolvement(string buildInvolvementId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<string>>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetBuildLogParseResultsForBuildInvolvement),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildInvolvementId", Value = buildInvolvementId }
                }
            });
        }

        #endregion

        #region DAEMONTASK

        DaemonTask IDataPlugin.SaveDaemonTask(DaemonTask daemonTask)
        {
            return _pluginSender.InvokeMethod<DaemonTask>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.SaveDaemonTask),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "daemonTask", Value = daemonTask }
                }
            });
        }

        DaemonTask IDataPlugin.GetDaemonTaskById(string id)
        {
            return _pluginSender.InvokeMethod<DaemonTask>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetDaemonTaskById),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        bool IDataPlugin.DeleteDaemonTask(DaemonTask record)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.DeleteDaemonTask),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "record", Value = record }
                }
            });
        }

        IEnumerable<DaemonTask> IDataPlugin.GetDaemonsTaskByBuild(string buildid)
        {
            return _pluginSender.InvokeMethod<IEnumerable<DaemonTask>>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetDaemonsTaskByBuild),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildid", Value = buildid }
                }
            });
        }

        IEnumerable<DaemonTask> IDataPlugin.GetPendingDaemonTasksByTask(string task)
        {
            return _pluginSender.InvokeMethod<IEnumerable<DaemonTask>>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetPendingDaemonTasksByTask),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "task", Value = task }
                }
            });
        }

        bool IDataPlugin.DaemonTasksBlocked(string buildId, int order)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.DaemonTasksBlocked),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildId", Value = buildId },
                    new PluginFunctionParameter { Name = "order", Value = order }
                }
            });
        }

        PageableData<DaemonTask> IDataPlugin.PageDaemonTasks(int index, int pageSize, string filterBy = "")
        {
            return _pluginSender.InvokeMethod<PageableData<DaemonTask>>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.PageDaemonTasks),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "index", Value = index },
                    new PluginFunctionParameter { Name = "pageSize", Value = pageSize },
                    new PluginFunctionParameter { Name = "filterBy", Value = filterBy }
                }
            });
        }

        #endregion

        #region BUILD INVOLVEMENT

        BuildInvolvement IDataPlugin.SaveBuildInvolement(BuildInvolvement buildInvolvement)
        {
            return _pluginSender.InvokeMethod<BuildInvolvement>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.SaveBuildInvolement),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildInvolvement", Value = buildInvolvement }
                }
            });
        }

        BuildInvolvement IDataPlugin.GetBuildInvolvementById(string id)
        {
            return _pluginSender.InvokeMethod<BuildInvolvement>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetBuildInvolvementById),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        BuildInvolvement IDataPlugin.GetBuildInvolvementByRevisionCode(string buildid, string revisionCode)
        {
            return _pluginSender.InvokeMethod<BuildInvolvement>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetBuildInvolvementByRevisionCode),
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
                FunctionName = nameof(IDataPlugin.DeleteBuildInvolvement),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "record", Value = record }
                }
            });
        }

        IEnumerable<BuildInvolvement> IDataPlugin.GetBuildInvolvementsByBuild(string buildId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<BuildInvolvement>>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetBuildInvolvementsByBuild),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildId", Value = buildId }
                }
            });
        }

        IEnumerable<BuildInvolvement> IDataPlugin.GetBuildInvolvementByUserId(string userId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<BuildInvolvement>>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetBuildInvolvementByUserId),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "userId", Value = userId }
                }
            });
        }

        PageableData<BuildInvolvement> IDataPlugin.PageBuildInvolvementsByUserAndStatus(string userid, BuildStatus buildStatus, int index, int pageSize)
        {
            return _pluginSender.InvokeMethod<PageableData<BuildInvolvement>>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.PageBuildInvolvementsByUserAndStatus),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "userid", Value = userid },
                    new PluginFunctionParameter { Name = "buildStatus", Value = buildStatus },
                    new PluginFunctionParameter { Name = "index", Value = index },
                    new PluginFunctionParameter { Name = "pageSize", Value = pageSize }
                }
            });
        }

        #endregion

        #region REVISION

        Revision IDataPlugin.SaveRevision(Revision revision)
        {
            return _pluginSender.InvokeMethod<Revision>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.SaveRevision),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "revision", Value = revision }
                }
            });
        }

        Revision IDataPlugin.GetRevisionById(string id)
        {
            return _pluginSender.InvokeMethod<Revision>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetRevisionById),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        Revision IDataPlugin.GetRevisionByKey(string sourceServerId, string key)
        {
            return _pluginSender.InvokeMethod<Revision>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetRevisionByKey),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "sourceServerId", Value = sourceServerId },
                    new PluginFunctionParameter { Name = "key", Value = key }
                }
            });
        }

        Revision IDataPlugin.GetNewestRevisionForBuild(string buildId)
        {
            return _pluginSender.InvokeMethod<Revision>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetNewestRevisionForBuild),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildId", Value = buildId }
                }
            });
        }

        IEnumerable<Revision> IDataPlugin.GetRevisionByBuild(string buildId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Revision>>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetRevisionByBuild),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildId", Value = buildId }
                }
            });
        }

        IEnumerable<Revision> IDataPlugin.GetRevisionsBySourceServer(string sourceServerId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Revision>>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetRevisionsBySourceServer),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "sourceServerId", Value = sourceServerId }
                }
            });
        }

        bool IDataPlugin.DeleteRevision(Revision revision)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.DeleteRevision),
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
                FunctionName = nameof(IDataPlugin.SaveSession),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "session", Value = session }
                }
            });
        }

        Session IDataPlugin.GetSessionById(string id)
        {
            return _pluginSender.InvokeMethod<Session>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetSessionById),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "id", Value = id }
                }
            });
        }

        IEnumerable<Session> IDataPlugin.GetSessionByUserId(string userid)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Session>>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetSessionByUserId),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "userid", Value = userid }
                }
            });
        }

        bool IDataPlugin.DeleteSession(Session session)
        {
            return _pluginSender.InvokeMethod<bool>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.DeleteSession),
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
                FunctionName = nameof(IDataPlugin.GetLastJobDelta),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "jobId", Value = jobId }
                }
            });
        }

        void IDataPlugin.SaveJobDelta(Build build)
        {
            _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.SaveJobDelta),
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
                FunctionName = nameof(IDataPlugin.AddConfigurationState),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "configurationState", Value = configurationState }
                }
            });
        }

        int IDataPlugin.ClearAllTables() 
        {
            return _pluginSender.InvokeMethod<int>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.ClearAllTables),
            });
        }

        ConfigurationState IDataPlugin.GetLatestConfigurationState()
        {
            return _pluginSender.InvokeMethod<ConfigurationState>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.GetLatestConfigurationState),
            });
        }

        PageableData<ConfigurationState> IDataPlugin.PageConfigurationStates(int index, int pageSize)
        {
            return _pluginSender.InvokeMethod<PageableData<ConfigurationState>>(this, new PluginArgs
            {
                FunctionName = nameof(IDataPlugin.PageConfigurationStates),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "index", Value = index },
                    new PluginFunctionParameter { Name = "pageSize", Value = pageSize }
                }
            });
        }

        #endregion
    }
}

