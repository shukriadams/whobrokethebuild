using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    public class Postgres : Plugin, IDataPlugin
    {
        #region FIELDS

        private readonly Configuration _config;

        public NpgsqlConnection Connection { get; private set; }

        private NpgsqlTransaction _transaction;

        string test = "";

        public string id = Guid.NewGuid().ToString();

        public string PubString = "";

        #endregion

        #region CTORS

        public Postgres(Configuration config) 
        { 
            _config = config;
        }

        #endregion

        #region UTIL

        void IDisposable.Dispose() 
        {
            if (this.Connection != null) 
                this.Connection.Dispose();
        }

        void IDataPlugin.TransactionStart()
        {
            if (_transaction != null)
                throw new Exception("Transaction has already been started with this data object");

            this.Connection = PostgresCommon.GetConnection(this.ContextPluginConfig);
            _transaction = this.Connection.BeginTransaction();
        }

        void IDataPlugin.TransactionCommit()
        {
            // getting bizarre cross-thread collisions on same instance
            if (_transaction == null)
                return;

                // throw new Exception("Cannot commit - transaction not set on this data object");

            if (_transaction != null)
            {
                _transaction.Commit();
                _transaction.Dispose();
            }

            if (this.Connection != null)
            {
                this.Connection.Dispose();
            }

            _transaction = null;
            this.Connection = null;
            this.test += $"committed | {this.id} | {new System.Diagnostics.StackTrace(true)}";
        }

        void IDataPlugin.TransactionCancel()
        {
            // getting bizarre cross-thread collisions on same instance
            if (_transaction == null)
                return;
                //throw new Exception("Cannot cancel - transaction not set on this data object");

            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
            }

            if (this.Connection != null)
            {
                this.Connection.Dispose();
            }

            _transaction = null;
            this.Connection = null;
            this.test += $"cancelled | {this.id} | {new System.Diagnostics.StackTrace(true)}";
        }

        PluginInitResult IPlugin.InitializePlugin()
        {
            if (this.ContextPluginConfig.Config == null)
                throw new ConfigurationException("Missing node \"Config\"");

            if (!this.ContextPluginConfig.Config.Any(c => c.Key == "Host"))
                throw new ConfigurationException("Missing config item \"Host\"");

            if (!this.ContextPluginConfig.Config.Any(c => c.Key == "Password"))
                throw new ConfigurationException("Missing config item \"Password\"");

            if (!this.ContextPluginConfig.Config.Any(c => c.Key == "User"))
                throw new ConfigurationException("Missing config item \"User\"");

            if (!this.ContextPluginConfig.Config.Any(c => c.Key == "Database"))
                throw new ConfigurationException("Missing config item \"Database\"");

            return new PluginInitResult{ 
                SessionId = Guid.NewGuid().ToString(),
                Success = true
            };
        }

        ReachAttemptResult IReachable.AttemptReach()
        {
            try 
            {
                PostgresCommon.ContactServer(this.ContextPluginConfig);
                return new ReachAttemptResult { Reachable = true };
            }
            catch(Exception ex)
            {
                return new ReachAttemptResult { Exception = ex };
            }
        }

        int IDataPlugin.InitializeDatastore()
        {
            return PostgresCommon.InitializeDatastore(this.ContextPluginConfig);
        }

        int IDataPlugin.DestroyDatastore() 
        {
            return PostgresCommon.DestroyDatastore(this.ContextPluginConfig);
        }

        #endregion

        #region STORE

        StoreItem IDataPlugin.SaveStore(StoreItem storeItem)
        {
            string insertQuery = @"
                INSERT INTO store
                    (key, plugin, content)
                VALUES
                    (@key, @plugin, @content)
                RETURNING id";

            string updateQuery = @"                    
                UPDATE store SET 
                    content = @content
                WHERE
                    id = @id";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            {
                if (string.IsNullOrEmpty(storeItem.Id))
                    storeItem.Id = PostgresCommon.InsertWithId(this.ContextPluginConfig, insertQuery, storeItem, new ParameterMapper<StoreItem>(StoreItemMapping.MapParameters), conWrap.Connection());
                else
                    PostgresCommon.Update(this.ContextPluginConfig, updateQuery, storeItem, new ParameterMapper<StoreItem>(StoreItemMapping.MapParameters), conWrap.Connection());

                return storeItem;
            }
        }
        
        StoreItem IDataPlugin.GetStoreItemByItem(string id)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.GetById(conWrap.Connection(), this.ContextPluginConfig, id, "store", new StoreItemConvert());
        }

        StoreItem IDataPlugin.GetStoreItemByKey(string key)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.GetByField(conWrap.Connection(), this.ContextPluginConfig, "key", key, "store", new StoreItemConvert());
        }

        bool IDataPlugin.DeleteStoreItem(StoreItem record)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.Delete(conWrap.Connection(), this.ContextPluginConfig, "store", "id", record.Id);
        }

        bool IDataPlugin.DeleteStoreItemWithKey(string key)
        {
            string query = @"
                DELETE FROM 
                    store 
                WHERE
                    key = @key";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("key", key);
                return cmd.ExecuteNonQuery() != 0;
            }
        }

        #endregion

        #region BUILD SERVER

        BuildServer IDataPlugin.SaveBuildServer(BuildServer buildServer)
        {
            string insertQuery = @"
                INSERT INTO buildserver
                    (key)
                VALUES
                    (@key)
                RETURNING id";

            string updateQuery = @"                    
                UPDATE buildserver SET 
                    key = @key
                WHERE
                    id = @id";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            {
                if (string.IsNullOrEmpty(buildServer.Id))
                    buildServer.Id = PostgresCommon.InsertWithId(this.ContextPluginConfig, insertQuery, buildServer, new ParameterMapper<BuildServer>(BuildServerMapping.MapParameters), conWrap.Connection());
                else
                    PostgresCommon.Update(this.ContextPluginConfig, updateQuery, buildServer, new ParameterMapper<BuildServer>(BuildServerMapping.MapParameters), conWrap.Connection());

                return buildServer;
            }
        }

        BuildServer IDataPlugin.GetBuildServerById(string id)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.GetById(conWrap.Connection(), this.ContextPluginConfig, id, "buildserver", new BuildServerConvert(_config));
        }

        BuildServer IDataPlugin.GetBuildServerByKey(string key)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.GetByField(conWrap.Connection(), this.ContextPluginConfig, "key", key, "buildserver", new BuildServerConvert(_config));
        }

        IEnumerable<BuildServer> IDataPlugin.GetBuildServers()
        {
            string query = @"
                    SELECT 
                        id,
                        key
                    FROM 
                        buildserver";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conWrap.Connection()))
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                return new BuildServerConvert(_config).ToCommonList(reader);
        }

        bool IDataPlugin.DeleteBuildServer(BuildServer record)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.Delete(conWrap.Connection(), this.ContextPluginConfig, "buildserver", "id", record.Id);
        }

        #endregion

        #region SOURCE SERVER

        SourceServer IDataPlugin.SaveSourceServer(SourceServer sourceServer)
        {
            string insertQuery = @"
                INSERT INTO sourceserver
                    (key)
                VALUES
                    (@key)
                RETURNING id";

            string updateQuery = @"                    
                UPDATE sourceserver SET 
                    key = @key
                WHERE
                    id = @id";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this)) 
            {
                if (string.IsNullOrEmpty(sourceServer.Id))
                    sourceServer.Id = PostgresCommon.InsertWithId(this.ContextPluginConfig, insertQuery, sourceServer, new ParameterMapper<SourceServer>(SourceServerMapping.MapParameters), conWrap.Connection());
                else
                    PostgresCommon.Update(this.ContextPluginConfig, updateQuery, sourceServer, new ParameterMapper<SourceServer>(SourceServerMapping.MapParameters), conWrap.Connection());

                return sourceServer;
            }
        }

        SourceServer IDataPlugin.GetSourceServerById(string id)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.GetById(conWrap.Connection(), this.ContextPluginConfig, id, "sourceserver", new SourceServerConvert(_config));
        }

        SourceServer IDataPlugin.GetSourceServerByKey(string key)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.GetByField(conWrap.Connection(), this.ContextPluginConfig, "key", key, "sourceserver", new SourceServerConvert(_config));
        }

        IEnumerable<SourceServer> IDataPlugin.GetSourceServers()
        {
            string query = @"
                SELECT 
                    id,
                    key
                FROM 
                    sourceserver";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conWrap.Connection()))
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                return new SourceServerConvert(_config).ToCommonList(reader);
        }

        bool IDataPlugin.DeleteSourceServer(SourceServer record)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.Delete(conWrap.Connection(), this.ContextPluginConfig, "sourceserver", "id", record.Id);
        }

        #endregion

        #region JOB
        
        Job IDataPlugin.SaveJob(Job job)
        {
            string insertQuery = @"
                INSERT INTO job
                    (key, buildserverid, sourceserverid)
                VALUES
                    (@key, @buildserverid, @sourceserverid)
                RETURNING id";

            string updateQuery = @"                    
                UPDATE job SET 
                    key = @key,
                    buildserverid = @buildserverid, 
                    sourceserverid = @sourceserverid
                WHERE
                    id = @id";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            {
                if (string.IsNullOrEmpty(job.Id))
                    job.Id = PostgresCommon.InsertWithId<Job>(this.ContextPluginConfig, insertQuery, job, new ParameterMapper<Job>(JobMapping.MapRevisionParameters), conWrap.Connection());
                else
                    PostgresCommon.Update<Job>(this.ContextPluginConfig, updateQuery, job, new ParameterMapper<Job>(JobMapping.MapRevisionParameters), conWrap.Connection());

                return job;
            }
        }

        Job IDataPlugin.GetJobById(string id)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.GetById<Job>(conWrap.Connection(), this.ContextPluginConfig, id, "job", new JobConvert(_config));
        }

        Job IDataPlugin.GetJobByKey(string key)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.GetByField(conWrap.Connection(), this.ContextPluginConfig, "key", key, "job", new JobConvert(_config));
        }

        IEnumerable<Job> IDataPlugin.GetJobsByBuildServerId(string buildServerId)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT * from job where buildserverid=@buildserverid", conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("buildserverid", int.Parse(buildServerId));

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new JobConvert(_config).ToCommonList(reader);
            }
        }

        IEnumerable<Job> IDataPlugin.GetJobs()
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT * from job", conWrap.Connection()))
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                return new JobConvert(_config).ToCommonList(reader);
        }

        bool IDataPlugin.DeleteJob(Job job)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.Delete(conWrap.Connection(), this.ContextPluginConfig, "job", "id", job.Id);
        }

        JobStats IDataPlugin.GetJobStats(Job job)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            {
                NpgsqlConnection connection = conWrap.Connection();

                JobStats stats = new JobStats();

                // first build
                string firstBuildQuery = @"
                SELECT
                    *
                FROM
                    build
                WHERE 
                    jobid = @jobid
                ORDER BY
                    startedutc ASC
                LIMIT 1
                ";

                Build firstBuild = PostgresCommon.GetByQuery<Build>(connection, this.ContextPluginConfig, firstBuildQuery, new[] { new QueryParameter("jobid", int.Parse(job.Id)) }, new BuildConvert());

                // latest build
                string latestBuildQuery = @"
                SELECT
                    *
                FROM
                    build
                WHERE 
                    jobid = @jobid
                ORDER BY
                    startedutc DESC
                LIMIT 1";

                stats.LatestBuild = PostgresCommon.GetByQuery<Build>(connection, this.ContextPluginConfig, latestBuildQuery, new[] { new QueryParameter("jobid", int.Parse(job.Id)) }, new BuildConvert());

                if (firstBuild != null)
                    stats.StartUtc = firstBuild.StartedUtc;

                if (firstBuild != null && stats.LatestBuild != null && stats.LatestBuild.EndedUtc != null)
                    stats.JobDuration = stats.LatestBuild.EndedUtc - firstBuild.StartedUtc;

                // latest broken build, this is a simply the last build with status = broken, not the build that broke the job
                string latestBrokenBuildQuery = @"
                SELECT
                    *
                FROM
                    build
                WHERE 
                    jobid = @jobid
                    and status = @failed
                ORDER BY
                    startedutc DESC
                LIMIT 1";

                stats.LatestBrokenBuild = PostgresCommon.GetByQuery<Build>(connection, this.ContextPluginConfig, latestBrokenBuildQuery, new[] {
                new QueryParameter("jobid", int.Parse(job.Id)),
                new QueryParameter("failed", (int)BuildStatus.Failed),
            }, new BuildConvert());

                // latest breaking build, ie, the last build that cause the job to fail
                // latest break duration
                if (stats.LatestBrokenBuild != null)
                {
                    if (!string.IsNullOrEmpty(stats.LatestBrokenBuild.IncidentBuildId))
                    {
                        if (stats.LatestBrokenBuild.Id == stats.LatestBrokenBuild.IncidentBuildId)
                            stats.LatestBreakingBuild = stats.LatestBrokenBuild;
                        else
                            stats.LatestBreakingBuild = ((IDataPlugin)this).GetBuildById(stats.LatestBrokenBuild.IncidentBuildId);

                        Build fixingBuild = ((IDataPlugin)this).GetFirstPassingBuildAfterBuild(stats.LatestBreakingBuild);
                        if (fixingBuild != null && fixingBuild.EndedUtc.HasValue && stats.LatestBreakingBuild.EndedUtc.HasValue)
                            stats.LatestBreakDuration = fixingBuild.EndedUtc.Value - stats.LatestBreakingBuild.EndedUtc.Value;
                    }
                }

                // total builds
                string buildCount = @"
                SELECT
                    COUNT(id)
                FROM
                    build
                WHERE
                    jobid = @jobid";

                using (NpgsqlCommand cmd = new NpgsqlCommand(buildCount, connection))
                {
                    cmd.Parameters.AddWithValue("jobid", int.Parse(job.Id));

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        stats.TotalBuilds = reader.GetInt32(0);
                    }
                }

                // total fails
                string failCountQuery = @"
                SELECT
                    COUNT(id)
                FROM
                    build
                WHERE
                    jobid = @jobid
                    AND (
                        status = @status_failed
                        OR status = @status_aborted
                    )";

                using (NpgsqlCommand cmd = new NpgsqlCommand(failCountQuery, connection))
                {
                    cmd.Parameters.AddWithValue("jobid", int.Parse(job.Id));
                    cmd.Parameters.AddWithValue("status_failed", (int)BuildStatus.Failed);
                    cmd.Parameters.AddWithValue("status_aborted", (int)BuildStatus.Aborted);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        stats.TotalFails = reader.GetInt32(0);
                    }
                }

                // total incidents
                string incidentCountQuery = @"
                SELECT COUNT(DISTINCT
                    incidentbuildid) 
                FROM 
                    build
                WHERE
                    jobid = @jobid";

                using (NpgsqlCommand cmd = new NpgsqlCommand(incidentCountQuery, connection))
                {
                    cmd.Parameters.AddWithValue("jobid", int.Parse(job.Id));
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        stats.Incidents = reader.GetInt32(0);
                    }
                }

                // fail rate
                stats.FailRatePercent = PercentHelper.ToPercent(stats.TotalFails, stats.TotalBuilds);

                // rate per day
                if (stats.JobDuration.HasValue && stats.JobDuration.Value.TotalDays > 0 && stats.TotalBuilds > 0)
                    stats.DailyBuildRate = (int)Math.Round((decimal)((decimal)stats.TotalBuilds / (decimal)stats.JobDuration.Value.TotalDays));

                // rate per week
                if (stats.JobDuration.HasValue && stats.JobDuration.Value.TotalDays > 0 && stats.TotalBuilds > 0)
                    stats.WeeklyBuildRate = (int)Math.Round((decimal)((decimal)stats.TotalBuilds / (decimal)stats.JobDuration.Value.TotalDays / 7));

                // longest uptime
                IEnumerable<string> incidentBuildIds = ((IDataPlugin)this).GetIncidentIdsForJob(job);

                // longest downtime

                return stats;
            }
        }

        int IDataPlugin.ResetJob(string jobId, bool hard)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this)) 
            {
                int affected = 0;
                // remove build involvements
                string removeBuildInvolvements = @"
                DELETE FROM 
                    buildinvolvement 
                USING 
                    build
                WHERE
                    build.id = buildinvolvement.buildid
                    AND build.jobid = @jobid";

                using (NpgsqlCommand cmd = new NpgsqlCommand(removeBuildInvolvements, conWrap.Connection()))
                {
                    cmd.Parameters.AddWithValue("jobid", int.Parse(jobId));
                    affected += cmd.ExecuteNonQuery();
                }

                // reset build log parsed result
                string logreset = @"
                DELETE FROM
                    buildlogparseresult
                USING
                    build
                WHERE
                    build.id = buildlogparseresult.buildid
                    AND build.jobid = @jobid";

                using (NpgsqlCommand cmd = new NpgsqlCommand(logreset, conWrap.Connection()))
                {
                    cmd.Parameters.AddWithValue("jobid", int.Parse(jobId));
                    affected += cmd.ExecuteNonQuery();
                }

                string resetDaemonTasks = @"
                DELETE FROM 
                    daemontask
                USING
                    build
                WHERE
                    daemontask.buildid = build.id
                    AND build.jobid = @jobid";

                using (NpgsqlCommand cmd = new NpgsqlCommand(resetDaemonTasks, conWrap.Connection()))
                {
                    cmd.Parameters.AddWithValue("jobid", int.Parse(jobId));
                    affected += cmd.ExecuteNonQuery();
                }

                string incidentReportReset = @"
                DELETE FROM
                    incidentreport
                USING
                    build
                WHERE
                    (incidentreport.incidentid = build.id
                    OR incidentreport.mutationid = build.id)
                    AND build.jobid = @jobid";

                using (NpgsqlCommand cmd = new NpgsqlCommand(incidentReportReset, conWrap.Connection()))
                {
                    cmd.Parameters.AddWithValue("jobid", int.Parse(jobId));
                    affected += cmd.ExecuteNonQuery();
                }

                if (hard)
                {
                    // delete all builds
                    string delete = @"
                    DELETE FROM 
                        build 
                    WHERE 
                        jobid = @jobid";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(delete, conWrap.Connection()))
                    {
                        cmd.Parameters.AddWithValue("jobid", int.Parse(jobId));
                        affected += cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    // clear incident builds 
                    string incidentReset = @"
                        UPDATE 
                            build 
                        SET
                            incidentbuildid = NULL
                        WHERE 
                            jobid = @jobid";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(incidentReset, conWrap.Connection()))
                    {
                        cmd.Parameters.AddWithValue("jobid", int.Parse(jobId));
                        affected += cmd.ExecuteNonQuery();
                    }
                }
                return affected;
            }
        }

        IEnumerable<string> IDataPlugin.GetIncidentIdsForJob(Job job)
        { 
            string query = @"
                SELECT
                    *
                FROM
                    build
                WHERE id IN (
                    SELECT DISTINCT
                        incidentbuildid 
                    FROM 
                        build
                    WHERE
                        jobid = @jobid
                )
                ORDER BY
                    startedutc DESC";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conWrap.Connection()))
            {
                IList<string> buildIds = new List<string>();
                cmd.Parameters.AddWithValue("jobid", int.Parse(job.Id));

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    while (reader.Read())
                        buildIds.Add(reader["incidentbuildid"].ToString());

                return buildIds;
            }
        }

        #endregion

        #region USER

        User IDataPlugin.SaveUser(User user)
        {
            string insertQuery = @"
                INSERT INTO usr
                    (key)
                VALUES
                    (@key)
                RETURNING id";

            string updateQuery = @"                    
                UPDATE usr SET 
                    key = @key
                WHERE
                    id = @id";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            {
                if (string.IsNullOrEmpty(user.Id))
                    user.Id = PostgresCommon.InsertWithId<User>(this.ContextPluginConfig, insertQuery, user, new ParameterMapper<User>(UserMapping.MapParameters), conWrap.Connection());
                else
                    PostgresCommon.Update<User>(this.ContextPluginConfig, updateQuery, user, new ParameterMapper<User>(UserMapping.MapParameters), conWrap.Connection());

                return user;
            }
        }

        User IDataPlugin.GetUserById(string id)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.GetById<User>(conWrap.Connection(), this.ContextPluginConfig, id, "usr", new UserConvert(_config));
        }

        User IDataPlugin.GetUserByKey(string key)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.GetByField(conWrap.Connection(), this.ContextPluginConfig, "key", key, "usr", new UserConvert(_config));
        }

        IEnumerable<User> IDataPlugin.GetUsers()
        {
            string query = @"
                    SELECT 
                        id,
                        key
                    FROM 
                        usr";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conWrap.Connection()))
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                return new UserConvert(_config).ToCommonList(reader);
        }

        PageableData<User> IDataPlugin.PageUsers(int index, int pageSize)
        {
            string pageQuery = @"
                SELECT 
                    id,
                    key
                FROM
                    usr
                LIMIT 
                    @pagesize
                OFFSET 
                    @index";

            string countQuery = @"
                SELECT 
                    COUNT(id)
                FROM
                    usr";

            long virtualItemCount = 0;
            IEnumerable<User> users = null;

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            {
                // get main records
                using (NpgsqlCommand cmd = new NpgsqlCommand(pageQuery, conWrap.Connection()))
                {
                    cmd.Parameters.AddWithValue("index", index * pageSize);
                    cmd.Parameters.AddWithValue("pagesize", pageSize);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        users = new UserConvert(_config).ToCommonList(reader);
                }

                // get count of total records possible
                using (NpgsqlCommand cmd = new NpgsqlCommand(countQuery, conWrap.Connection()))
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    virtualItemCount = reader.GetInt64(0);
                }

                return new PageableData<User>(users, index, pageSize, virtualItemCount);
            }
        }

        bool IDataPlugin.DeleteUser(User record)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.Delete(conWrap.Connection(), this.ContextPluginConfig, "usr", "id", record.Id);
        }

        #endregion

        #region BUILD

        /// <summary>
        /// 
        /// </summary>
        /// <param name="build"></param>
        /// <returns></returns>
        Build IDataPlugin.SaveBuild(Build build)
        {
            string insertQuery = @"
                INSERT INTO build
                    (jobid, signature, identifier, logpath, incidentbuildid, triggeringcodechange, triggeringtype, startedutc, endedutc, hostname, status)
                VALUES
                    (@jobid, @signature, @identifier, @logpath, @incidentbuildid, @triggeringcodechange, @triggeringtype, @startedutc, @endedutc, @hostname, @status)
                RETURNING id";

            string updateQuery = @"                    
                UPDATE build SET 
                    jobid = @jobid, 
                    signature = @new_signature,
                    identifier = @identifier, 
                    incidentbuildid = @incidentbuildid,
                    logpath = @logpath,
                    triggeringcodechange = @triggeringcodechange, 
                    triggeringtype = @triggeringtype, 
                    startedutc = @startedutc, 
                    endedutc = @endedutc, 
                    hostname = @hostname, 
                    status = @status
                WHERE
                    id = @id
                    AND signature = @signature";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            {
                if (string.IsNullOrEmpty(build.Id))
                    build.Id = PostgresCommon.InsertWithId<Build>(this.ContextPluginConfig, insertQuery, build, new ParameterMapper<Build>(BuildMapping.MapParameters), conWrap.Connection());
                else
                    PostgresCommon.Update<Build>(this.ContextPluginConfig, updateQuery, build, new ParameterMapper<Build>(BuildMapping.MapParameters), conWrap.Connection());

                return build;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Build IDataPlugin.GetBuildById(string id)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.GetById<Build>(conWrap.Connection(), this.ContextPluginConfig, id, "build", new BuildConvert());
        }

        public Build GetCurrentBuildDelta(string jobId)
        {
            string query = @"
                SELECT 
	                *
                FROM
	                build B
                WHERE
	                B.jobid=1
	                AND B.status = (
		                -- get current passing or failing status of job 
		                SELECT 
			                status 
		                FROM 
			                build
		                WHERE 
			                status = 3
			                OR status = 4
		                ORDER BY 
			                endedutc DESC
		                LIMIT 1
	                ) AND b.endedutc > (
		                -- get endutc of last build of previous status
		                SELECT 
			                endedutc 
		                FROM 
			                build
		                WHERE 
			                NOT endedutc IS NULL
		                    AND (status = @passing
		                    OR status = @failing)
			                AND NOT status = (
		                        -- get current passing or failing status of job 
		                        SELECT 
			                        status 
		                        FROM 
			                        build
		                        WHERE 
			                        status = @passing
			                        OR status = @failing
		                        ORDER BY 
			                        endedutc DESC
		                        LIMIT 1)
		                ORDER BY 
			                endedutc DESC
		                LIMIT 1)
                ORDER BY 
	                endedutc ASC
                LIMIT 1";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("jobid", int.Parse(jobId));
                cmd.Parameters.AddWithValue("passing", (int)BuildStatus.Passed);
                cmd.Parameters.AddWithValue("failing", (int)BuildStatus.Failed);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new BuildConvert().ToCommon(reader);
            }
        }

        Build IDataPlugin.GetBuildByKey(string jobId, string key)
        {
            string query = @"
                SELECT 
                    *
                FROM
                    build 
                WHERE
                    jobid=@jobid
                    AND identifier=@key
                    ";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("jobid", int.Parse(jobId));
                cmd.Parameters.AddWithValue("key", key);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new BuildConvert().ToCommon(reader);
            }
        }

        PageableData<Build> IDataPlugin.PageIncidentsByJob(string jobId, int index, int pageSize)
        {
            // TODO : refactor this out. Also, this query will fail 
            string pageQuery = @"
                SELECT
                    *
                FROM
                    build
                WHERE id IN (
                    SELECT DISTINCT
                        incidentbuildid 
                    FROM 
                        build
                    WHERE
                        jobid = @jobid
                )
                ORDER BY
                    startedutc DESC";

            string countQuery = @"
                SELECT 
                    COUNT(DISTINCT incidentbuildid )
                FROM 
                    build
                WHERE
                    jobid = @jobid";

            long virtualItemCount = 0;
            IEnumerable<Build> builds = null;
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            {
                // get main records
                using (NpgsqlCommand cmd = new NpgsqlCommand(pageQuery, conWrap.Connection()))
                {
                    cmd.Parameters.AddWithValue("jobid", int.Parse(jobId));
                    cmd.Parameters.AddWithValue("index", index * pageSize);
                    cmd.Parameters.AddWithValue("pagesize", pageSize);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        builds = new BuildConvert().ToCommonList(reader);
                }

                // get count of total records possible
                using (NpgsqlCommand cmd = new NpgsqlCommand(countQuery, conWrap.Connection()))
                {
                    cmd.Parameters.AddWithValue("jobid", int.Parse(jobId));

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        virtualItemCount = reader.GetInt64(0);
                    }
                }

                return new PageableData<Build>(builds, index, pageSize, virtualItemCount);
            }
        }

        PageableData<Build> IDataPlugin.PageBuildsByJob(string jobId, int index, int pageSize, bool sortAscending)
        {
            string sortOrder = "";
            if (!sortAscending)
                sortOrder = "DESC";

            string pageQuery = @"
                SELECT 
                    *
                FROM
                    build 
                WHERE
                    jobid = @jobid
                ORDER BY
                    startedutc "+ sortOrder  + @"
                LIMIT 
                    @pagesize
                OFFSET 
                    @index";

            string countQuery = @"
                SELECT 
                    COUNT(*)
                FROM
                    build
                WHERE
                    jobid = @jobid";

            long virtualItemCount = 0;
            IEnumerable<Build> builds = null;

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            {
                // get main records
                using (NpgsqlCommand cmd = new NpgsqlCommand(pageQuery, conWrap.Connection()))
                {
                    cmd.Parameters.AddWithValue("jobid", int.Parse(jobId));
                    cmd.Parameters.AddWithValue("index", index * pageSize);
                    cmd.Parameters.AddWithValue("pagesize", pageSize);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        builds = new BuildConvert().ToCommonList(reader);
                }

                // get count of total records possible
                using (NpgsqlCommand cmd = new NpgsqlCommand(countQuery, conWrap.Connection()))
                {
                    cmd.Parameters.AddWithValue("jobid", int.Parse(jobId));

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        virtualItemCount = reader.GetInt64(0);
                    }
                }

                return new PageableData<Build>(builds, index, pageSize, virtualItemCount);
            }
        }

        IEnumerable<Build> IDataPlugin.GetBuildsByIncident(string incidentId) 
        {
            string query = @"
                SELECT 
                    *
                FROM
                    build 
                WHERE
                    incidentBuildId = @incidentId
                    AND NOT id = @incidentId
                ORDER BY
                    startedutc DESC
                LIMIT 
                    100";

            

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conWrap.Connection())) 
            {
                cmd.Parameters.AddWithValue("incidentId", int.Parse(incidentId));
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new BuildConvert().ToCommonList(reader);
            }
        }

        PageableData<Build> IDataPlugin.PageBuildsByBuildAgent(string hostname, int index, int pageSize)
        {
            string pageQuery = @"
                SELECT 
                    *
                FROM
                    build 
                WHERE
                    hostname = @hostname
                ORDER BY
                    startedutc
                LIMIT 
                    @pagesize
                OFFSET 
                    @index";

            string countQuery = @"
                SELECT 
                    COUNT(id)
                FROM
                    build
                WHERE
                    hostname = @hostname";

            long virtualItemCount = 0;
            IEnumerable<Build> builds = null;

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            {
                // get main records
                using (NpgsqlCommand cmd = new NpgsqlCommand(pageQuery, conWrap.Connection()))
                {
                    cmd.Parameters.AddWithValue("hostname", hostname);
                    cmd.Parameters.AddWithValue("index", index * pageSize);
                    cmd.Parameters.AddWithValue("pagesize", pageSize);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        builds = new BuildConvert().ToCommonList(reader);
                }

                // get count of total records possible
                using (NpgsqlCommand cmd = new NpgsqlCommand(countQuery, conWrap.Connection()))
                {
                    cmd.Parameters.AddWithValue("hostname", hostname);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        virtualItemCount = reader.GetInt64(0);
                    }
                }

                return new PageableData<Build>(builds, index, pageSize, virtualItemCount);
            }
        }

        bool IDataPlugin.DeleteBuild(Build record)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.Delete(conWrap.Connection(), this.ContextPluginConfig, "build", "id", record.Id);
        }

        Build IDataPlugin.GetLatestBuildByJob(Job job)
        {
            string query = @"
                SELECT 
                    *
                FROM
                    build
                WHERE
                    jobid = @jobid
                ORDER BY
                    startedutc DESC
                LIMIT
                    1";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("jobid", int.Parse(job.Id));

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new BuildConvert().ToCommon(reader);
            }
        }

        Build IDataPlugin.GetDeltaBuildAtBuild(Build build)
        {
            // get first build in job that has the same status as referenceBuild
            string query = @"
                SELECT
                    *
                FROM
                    build
                WHERE
                    jobid = @jobid
                    AND status = @status
                    AND id <= @buildid
                    AND id > (
                        SELECT
                            id
                        FROM
                            build
                        WHERE
                            jobid = @jobid
                            AND (status = @failing OR status = @passing)
                            AND NOT status = @status
                            AND id < @buildid
                        ORDER BY
                            startedutc DESC
                        LIMIT
                            1
                    )
                ORDER BY
                    startedutc ASC
                LIMIT
                    1";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("jobid", int.Parse(build.JobId));
                cmd.Parameters.AddWithValue("buildid", int.Parse(build.Id));
                cmd.Parameters.AddWithValue("status", (int)build.Status);
                cmd.Parameters.AddWithValue("passing", (int)BuildStatus.Passed);
                cmd.Parameters.AddWithValue("failing", (int)BuildStatus.Failed);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new BuildConvert().ToCommon(reader);
            }
        }

        Build IDataPlugin.GetFirstPassingBuildAfterBuild(Build build)
        {
            string query = @"
                SELECT                     
                    *
                FROM 
                    build
                WHERE 
                    startedutc > (
                        SELECT startedutc FROM build WHERE id = @referencebuildid
                    )
                    AND jobid = @jobid
                    AND status = @build_passing
                ORDER BY 
                    startedutc 
                LIMIT 1";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("jobid", int.Parse(build.JobId));
                cmd.Parameters.AddWithValue("referencebuildid", int.Parse(build.Id));
                cmd.Parameters.AddWithValue("build_passing", (int)BuildStatus.Passed);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new BuildConvert().ToCommon(reader);
            }
        }

        Build IDataPlugin.GetPreviousBuild(Build build)
        {
            string query = @"
                SELECT                     
                    *
                FROM 
                    build
                WHERE 
                    startedutc < @buildDate
                    AND jobid = @jobid
                ORDER BY 
                    startedutc DESC
                LIMIT 1";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("jobid", int.Parse(build.JobId));
                cmd.Parameters.AddWithValue("buildDate", build.StartedUtc);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new BuildConvert().ToCommon(reader);
            }
        }

        Build IDataPlugin.GetNextBuild(Build build)
        {
            string query = @"
                SELECT                     
                   *
                FROM 
                    build
                WHERE 
                    startedutc > @buildDate
                    AND jobid = @jobid
                ORDER BY 
                    startedutc ASC
                LIMIT 1";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("jobid", int.Parse(build.JobId));
                cmd.Parameters.AddWithValue("buildDate", build.StartedUtc);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new BuildConvert().ToCommon(reader);
            }
        }

        int IDataPlugin.ResetBuild(string buildId, bool hard)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this)) 
            {
                int affected = 0;

                // remove build involvements
                string removeBuildInvolvements = @"
                DELETE FROM 
                    buildinvolvement 
                WHERE
                    buildid = @buildid";

                using (NpgsqlCommand cmd = new NpgsqlCommand(removeBuildInvolvements, conWrap.Connection()))
                {
                    cmd.Parameters.AddWithValue("buildid", int.Parse(buildId));
                    affected += cmd.ExecuteNonQuery();
                }

                string resetDaemonTasks = @"
                DELETE FROM 
                    daemontask
                USING
                    build
                WHERE
                    daemontask.buildid = build.id
                    AND build.id = @buildid";

                using (NpgsqlCommand cmd = new NpgsqlCommand(resetDaemonTasks, conWrap.Connection()))
                {
                    cmd.Parameters.AddWithValue("buildid", int.Parse(buildId));
                    affected += cmd.ExecuteNonQuery();
                }

                // reset build log parsed result
                string logreset = @"
                DELETE FROM
                    buildlogparseresult
                WHERE
                    buildid = @buildid";

                using (NpgsqlCommand cmd = new NpgsqlCommand(logreset, conWrap.Connection()))
                {
                    cmd.Parameters.AddWithValue("buildid", int.Parse(buildId));
                    affected += cmd.ExecuteNonQuery();
                }

                string incidentReportReset = @"
                DELETE FROM
                    incidentreport
                WHERE
                    incidentid = @buildid
                    OR mutationid = @buildid";

                using (NpgsqlCommand cmd = new NpgsqlCommand(incidentReportReset, conWrap.Connection()))
                {
                    cmd.Parameters.AddWithValue("buildid", int.Parse(buildId));
                    affected += cmd.ExecuteNonQuery();
                }


                if (hard)
                {
                    // delete all builds
                    string delete = @"
                        DELETE FROM 
                            build 
                        WHERE 
                            id = @buildid";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(delete, conWrap.Connection()))
                    {
                        cmd.Parameters.AddWithValue("buildid", int.Parse(buildId));
                        affected += cmd.ExecuteNonQuery();
                    }

                }
                else
                {
                    // remove incident builds from other builds
                    string incidentReset = @"
                        UPDATE 
                            build 
                        SET
                            incidentbuildid = NULL
                        WHERE 
                            id = @buildid";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(incidentReset, conWrap.Connection()))
                    {
                        cmd.Parameters.AddWithValue("buildid", int.Parse(buildId));
                        affected += cmd.ExecuteNonQuery();
                    }
                }

                return affected;
            }
        }

        #endregion

        #region BUILDLOGPARSERESULT

        BuildLogParseResult IDataPlugin.SaveBuildLogParseResult(BuildLogParseResult buildLog)
        {
            string insertQuery = @"
                INSERT INTO buildlogparseresult
                    (buildid, signature, logparserplugin, parsedcontent)
                VALUES
                    (@buildid, @signature, @logparserplugin, @parsedcontent)
                RETURNING id";

            string updateQuery = @"                    
                UPDATE buildlogparseresult SET 
                    buildid = @buildid, 
                    signature = @new_signature,
                    logparserplugin = @logparserplugin, 
                    parsedcontent = @parsedcontent
                WHERE
                    id = @id
                    AND signature = @signature";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            {
                if (string.IsNullOrEmpty(buildLog.Id))
                    buildLog.Id = PostgresCommon.InsertWithId<BuildLogParseResult>(this.ContextPluginConfig, insertQuery, buildLog, new ParameterMapper<BuildLogParseResult>(BuildLogParseResultMapping.MapParameters), conWrap.Connection());
                else
                    PostgresCommon.Update<BuildLogParseResult>(this.ContextPluginConfig, updateQuery, buildLog, new ParameterMapper<BuildLogParseResult>(BuildLogParseResultMapping.MapParameters), conWrap.Connection());

                return buildLog;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="buildId"></param>
        /// <returns></returns>
        IEnumerable<BuildLogParseResult> IDataPlugin.GetBuildLogParseResultsByBuildId(string buildId)
        {
            string query = @"
                SELECT 
                    BLPR.*,
                    R.buildinvolvementid
                FROM 
                    buildlogparseresult BLPR 
                    LEFT JOIN r_buildlogparseresult_buildinvolvement R ON R.buildlogparseresultid = BLPR.id
                WHERE
                    BLPR.buildid=@buildid";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("buildid", int.Parse(buildId));

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new BuildLogParseResultConvert().ToCommonList(reader);
            }
        }

        bool IDataPlugin.DeleteBuildLogParseResult(BuildLogParseResult result)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.Delete(conWrap.Connection(), this.ContextPluginConfig, "buildlogparseresult", "id", result.Id);
        }

        #endregion

        #region BUILDINVOLVEMENT

        BuildInvolvement IDataPlugin.SaveBuildInvolement(BuildInvolvement buildInvolvement)
        {
            string insertQuery = @"
                INSERT INTO buildinvolvement
                    (buildid, signature, revisioncode, revisionid, mappeduserid, isignoredfrombreakhistory, inferredrevisionlink, comment, blamescore)
                VALUES
                    (@buildid, @signature, @revisioncode, @revisionid, @mappeduserid, @isignoredfrombreakhistory, @inferredrevisionlink, @comment, @blamescore)
                RETURNING id";

            string updateQuery = @"                    
                UPDATE buildinvolvement SET 
                    buildid = @buildid,
                    signature = @new_signature,
                    revisioncode = @revisioncode,
                    revisionid = @revisionid, 
                    mappeduserid = @mappeduserid, 
                    isignoredfrombreakhistory = @isignoredfrombreakhistory, 
                    inferredrevisionlink = @inferredrevisionlink,
                    comment = @comment,
                    blamescore = @blamescore
                WHERE
                    id = @id
                    AND signature = @signature";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            {
                if (string.IsNullOrEmpty(buildInvolvement.Id))
                    buildInvolvement.Id = PostgresCommon.InsertWithId<BuildInvolvement>(this.ContextPluginConfig, insertQuery, buildInvolvement, new ParameterMapper<BuildInvolvement>(BuildInvolvementMapping.MapParameters), conWrap.Connection());
                else
                    PostgresCommon.Update<BuildInvolvement>(this.ContextPluginConfig, updateQuery, buildInvolvement, new ParameterMapper<BuildInvolvement>(BuildInvolvementMapping.MapParameters), conWrap.Connection());

                return buildInvolvement;
            }
        }

        BuildInvolvement IDataPlugin.GetBuildInvolvementById(string id)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.GetById<BuildInvolvement>(conWrap.Connection(), this.ContextPluginConfig, id, "buildinvolvement", new BuildInvolvementConvert());
        }

        BuildInvolvement IDataPlugin.GetBuildInvolvementByRevisionCode(string buildid, string revisionCode)
        {
            string sql = @"
                SELECT
                    *
                FROM
                    buildinvolvement
                WHERE
                    buildid = @buildid
                    AND revisioncode = @revisioncode";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("buildid", int.Parse(buildid));
                cmd.Parameters.AddWithValue("revisioncode", revisionCode);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new BuildInvolvementConvert().ToCommon(reader);
            }
        }

        bool IDataPlugin.DeleteBuildInvolvement(BuildInvolvement record)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.Delete(conWrap.Connection(), this.ContextPluginConfig, "buildinvolvement", "id", record.Id);
        }

        IEnumerable<BuildInvolvement> IDataPlugin.GetBuildInvolvementsByBuild(string buildId)
        {
            string sql = @"
                SELECT 
                    * 
                FROM 
                    buildinvolvement 
                WHERE 
                    buildid=@buildid";
            
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("buildid", int.Parse(buildId));

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new BuildInvolvementConvert().ToCommonList(reader);
            }
        }

        IEnumerable<BuildInvolvement> IDataPlugin.GetBuildInvolvementByUserId(string userId)
        {
            string sql = @"
                SELECT 
                    * 
                FROM 
                    buildinvolvement 
                WHERE 
                    mappeduserid = @userid";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("userid", int.Parse(userId));

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new BuildInvolvementConvert().ToCommonList(reader);
            }
        }

        PageableData<BuildInvolvement> IDataPlugin.PageBuildInvolvementsByUserAndStatus(string userid, BuildStatus buildStatus, int index, int pageSize)
        { 
            string sql = @"
                SELECT
                    BI.*
                FROM 
                    buildinvolvement BI
                    JOIN build B ON B.id = BI.buildid
                WHERE 
                    B.status = @buildstatus
                    AND BI.mappeduserid = @userid
                ORDER BY
                    B.startedutc DESC
                LIMIT 
                    @pagesize
                OFFSET 
                    @index";

            string countQuery = @"
                SELECT 
                    COUNT(BI.id)
                FROM 
                    buildinvolvement BI
                    JOIN build B ON B.id = BI.buildid
                WHERE 
                    B.status = @buildstatus
                    AND BI.mappeduserid = @userid";

            long virtualItemCount = 0;
            IEnumerable<BuildInvolvement> buildInvolvements = null;

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            {
                // get main
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conWrap.Connection()))
                {
                    cmd.Parameters.AddWithValue("userid", int.Parse(userid));
                    cmd.Parameters.AddWithValue("buildstatus", (int)buildStatus);
                    cmd.Parameters.AddWithValue("index", index);
                    cmd.Parameters.AddWithValue("pageSize", pageSize);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        buildInvolvements = new BuildInvolvementConvert().ToCommonList(reader);
                }

                // get count of total records possible
                using (NpgsqlCommand cmd = new NpgsqlCommand(countQuery, conWrap.Connection()))
                {
                    cmd.Parameters.AddWithValue("userid", int.Parse(userid));
                    cmd.Parameters.AddWithValue("buildstatus", (int)buildStatus);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        virtualItemCount = reader.GetInt64(0);
                    }
                }

                return new PageableData<BuildInvolvement>(buildInvolvements, index, pageSize, virtualItemCount);
            }
        }

        #endregion

        #region DAEMONTASK
        
        DaemonTask IDataPlugin.SaveDaemonTask(DaemonTask daemonTask)
        {
            string insertQuery = @"
                INSERT INTO daemontask
                    (buildid, signature, buildinvolvementid, src, createdutc, processedutc, passed, args, result, stage)
                VALUES
                    (@buildid, @signature,  @buildinvolvementid, @src, @createdutc, @processedutc, @passed, @args, @result, @stage)
                RETURNING id";

            string updateQuery = @"                    
                UPDATE daemontask SET 
                    buildid = @buildid,
                    signature = @new_signature,
                    buildinvolvementid = @buildinvolvementid, 
                    src = @src, 
                    stage = @stage,
                    createdutc = @createdutc,
                    processedutc = @processedutc,
                    passed = @passed,
                    args = @args,
                    result = @result
                WHERE
                    id = @id
                    AND signature = @signature";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            {
                if (string.IsNullOrEmpty(daemonTask.Id))
                    daemonTask.Id = PostgresCommon.InsertWithId<DaemonTask>(this.ContextPluginConfig, insertQuery, daemonTask, new ParameterMapper<DaemonTask>(DaemonTaskMapping.MapParameters), conWrap.Connection());
                else
                    PostgresCommon.Update<DaemonTask>(this.ContextPluginConfig, updateQuery, daemonTask, new ParameterMapper<DaemonTask>(DaemonTaskMapping.MapParameters), conWrap.Connection());

                return daemonTask;
            }
        }

        DaemonTask IDataPlugin.GetDaemonTaskById(string id)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.GetById<DaemonTask>(conWrap.Connection(), this.ContextPluginConfig, id, "daemontask", new DaemonTaskConvert());
        }

        bool IDataPlugin.DeleteDaemonTask(DaemonTask record)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.Delete(conWrap.Connection(), this.ContextPluginConfig, "daemontask", "id", record.Id);
        }

        IEnumerable<DaemonTask> IDataPlugin.GetDaemonsTaskByBuild(string buildid)
        {
            string sql = @"
                SELECT
                    *
                FROM
                    daemontask
                WHERE
                    buildid = @buildid";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("buildid", int.Parse(buildid));

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new DaemonTaskConvert().ToCommonList(reader);
            }
        }

        IEnumerable<DaemonTask> IDataPlugin.GetPendingDaemonTasksByTask(int stage) 
        {
            string sql = @"
                SELECT
                    *
                FROM
                    daemontask
                WHERE
                    stage = @stage
                    AND processedutc IS NULL
                ORDER BY
                    createdutc ASC
                LIMIT 
                    100";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("stage", stage);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new DaemonTaskConvert().ToCommonList(reader);
            }
        }

        IEnumerable<DaemonTask> IDataPlugin.DaemonTasksBlocked(string buildId, int order)
        {
            string sql = @"
                SELECT
                    *
                FROM
                    daemontask
                WHERE
                    buildid = @buildid
                    AND (
                        processedUtc is NULL
                        OR passed = False
                    )
                    AND stage < @order
                LIMIT 100";

            // note the hard limit, this call scales badly, and generally we don't care about the specifics of blocks once we have too many, as that's a symptom of bigger problems

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("buildid", int.Parse(buildId));
                cmd.Parameters.AddWithValue("order", order);

                using (NpgsqlDataReader reader = cmd.ExecuteReader()) 
                    return new DaemonTaskConvert().ToCommonList(reader);
            }
        }

        IEnumerable<DaemonTask> IDataPlugin.DaemonTasksBlockedForJob(string jobid, int order) 
        {
            string sql = @"
                SELECT
                    DT.*
                FROM
                    daemontask DT
                    JOIN build B on B.id = DT.buildid
                WHERE
                    B.jobid = @jobid
                    AND (
                        DT.processedUtc is NULL
                        OR DT.passed = False
                    )
                    AND DT.stage < @order
                LIMIT 100";

            // note the hard limit, this call scales badly, and generally we don't care about the specifics of blocks once we have too many, as that's a symptom of bigger problems

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("jobid", int.Parse(jobid));
                cmd.Parameters.AddWithValue("order", order);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new DaemonTaskConvert().ToCommonList(reader);
            }
        }

        PageableData<DaemonTask> IDataPlugin.PageDaemonTasks(int index, int pageSize, string orderBy = "", string filterBy = "", string jobId = "") 
        {
            /*
                filterBy options : 
                - unprocessed
                - failed
                - passed
                
             */

            string where = string.Empty;
            bool compountWhere = false;
            if (filterBy == "unprocessed")
            {
                where = " WHERE DT.processedutc IS NULL ";
                compountWhere = true;
            }

            if (filterBy == "failed")
            {
                where = " WHERE DT.passed = false ";
                compountWhere = true;
            }

            if (filterBy == "passed")
            {
                where = " WHERE DT.passed = true";
                compountWhere = true;
            }

            string sort = "DT.createdutc DESC";
            if (orderBy == "latestDone") 
            {
                sort = "DT.processedUtc DESC";
                where = "WHERE NOT DT.processedUtc IS NULL";
                compountWhere = true;
            }

            if (!string.IsNullOrEmpty(jobId)) 
            {
                if (compountWhere) 
                    where = $" {where} AND B.jobid={jobId} ";
                else
                    where = $" WHERE B.jobid={jobId} ";
            }

            if (orderBy == "oldest")
                sort = "createdutc ASC";

            string pageQuery = @"
                SELECT
                    DT.*
                FROM
                    daemontask DT
                    LEFT JOIN build B on B.id = DT.buildid
                    " + where+@"
                ORDER BY
                    "+sort+@"
                LIMIT 
                    @pagesize
                OFFSET 
                    @index";

            string countQuery = @"
                SELECT 
                    COUNT(DT.id)
                FROM 
                    daemontask DT
                    LEFT JOIN build B on B.id = DT.buildid
                " + where;

            long virtualItemCount = 0;
            IEnumerable<DaemonTask> builds = null;
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            {
                // get main records
                using (NpgsqlCommand cmd = new NpgsqlCommand(pageQuery, conWrap.Connection()))
                {
                    cmd.Parameters.AddWithValue("index", index * pageSize);
                    cmd.Parameters.AddWithValue("pagesize", pageSize);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        builds = new DaemonTaskConvert().ToCommonList(reader);
                }

                // get count of total records possible
                using (NpgsqlCommand cmd = new NpgsqlCommand(countQuery, conWrap.Connection()))
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    virtualItemCount = reader.GetInt64(0);
                }

                return new PageableData<DaemonTask>(builds, index, pageSize, virtualItemCount);
            }
        }

        #endregion

        #region INCIDENTREPORT

        IncidentReport IDataPlugin.SaveIncidentReport(IncidentReport incidentReport)
        {
            // enforce string lengths
            incidentReport.Processor = incidentReport.Processor.Length > 256 ? incidentReport.Processor.Substring(0, 256) : incidentReport.Processor;
            incidentReport.Summary = incidentReport.Summary.Length > 256 ? incidentReport.Summary.Substring(0, 256) : incidentReport.Summary;
            incidentReport.Status = incidentReport.Status.Length > 64 ? incidentReport.Status.Substring(0, 64) : incidentReport.Status;

            string insertQuery = @"
                INSERT INTO incidentreport
                    (incidentid, mutationid, signature, createdutc, status, summary, description, processor)
                VALUES
                    (@incidentid, @mutationid, @signature, @createdutc, @status, @summary, @description, @processor)
                RETURNING id";

            string updateQuery = @"                    
                UPDATE incidentreport SET 
                    signature = @new_signature,
                    incidentid = @incidentid,
                    mutationid = @mutationid,
                    signature = @signature,
                    createdutc = @createdutc,
                    status = @status,
                    summary = @summary,
                    description = @description,
                    processor = @processor
                WHERE
                    id = @id
                    AND signature = @signature";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            {
                if (string.IsNullOrEmpty(incidentReport.Id))
                    incidentReport.Id = PostgresCommon.InsertWithId<IncidentReport>(this.ContextPluginConfig, insertQuery, incidentReport, new ParameterMapper<IncidentReport>(IncidentReportMapping.MapParameters), conWrap.Connection());
                else
                    PostgresCommon.Update<IncidentReport>(this.ContextPluginConfig, updateQuery, incidentReport, new ParameterMapper<IncidentReport>(IncidentReportMapping.MapParameters), conWrap.Connection());

                return incidentReport;
            }
        }

        IncidentReport IDataPlugin.GetIncidentReportByMutation(string buildId) 
        {
            string sql = @"
                SELECT
                    *
                FROM
                    incidentreport
                WHERE
                    mutationid = @buildid
                ORDER BY
                    createdutc DESC
                LIMIT 1";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("buildid", int.Parse(buildId));

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new IncidentReportConvert().ToCommon(reader);
            }
        }

        IncidentReport IDataPlugin.GetIncidentReportById(string id)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.GetById<IncidentReport>(conWrap.Connection(), this.ContextPluginConfig, id, "incidentreport", new IncidentReportConvert());
        }

        bool IDataPlugin.DeleteIncidentReport(IncidentReport record)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.Delete(conWrap.Connection(), this.ContextPluginConfig, "incidentreport", "id", record.Id);
        }

        IEnumerable<IncidentReport> IDataPlugin.GetIncidentReportsForBuild(string buildId)
        {
            string sql = @"
                SELECT
                    *
                FROM
                    incidentreport
                WHERE
                    incidentId = @buildid
                ORDER BY
                    createdutc DESC";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("buildid", int.Parse(buildId));

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new IncidentReportConvert().ToCommonList(reader);
            }
        }

        #endregion

        #region R_BuildLogParseResult_BuildInvolvement

        string IDataPlugin.ConnectBuildLogParseResultAndBuildBuildInvolvement(string buildLogParseResultId, string buildInvolvementId)
        {
            string insertQuery = @"
                INSERT INTO r_buildLogParseResult_buildinvolvement
                    (buildlogparseresultid, buildinvolvementid)
                VALUES
                    (@buildLogParseResultId, @buildInvolvementId)
                RETURNING id";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(insertQuery, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("buildLogParseResultId", int.Parse(buildLogParseResultId));
                cmd.Parameters.AddWithValue("buildInvolvementId", int.Parse(buildInvolvementId));

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    return reader.GetInt32(0).ToString();
                }
            }
        }

        bool IDataPlugin.SplitBuildLogParseResultAndBuildBuildInvolvement(string id)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.Delete(conWrap.Connection(), this.ContextPluginConfig, "r_buildLogParseResult_buildinvolvement", "id", id);
        }

        IEnumerable<string> IDataPlugin.GetBuildLogParseResultsForBuildInvolvement(string buildInvolvementId) 
        {
            string query = @"
                SELECT
                    buildlogparseresultid
                FROM
                    r_buildLogParseResult_buildinvolvement
                WHERE
                    buildinvolvementid=@buildinvolvementid
                ";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("buildinvolvementid", int.Parse(buildInvolvementId));

                using (NpgsqlDataReader reader = cmd.ExecuteReader()) 
                {
                    IList<string> ids = new List<string>();
                    while (reader.Read())
                        ids.Add(reader["buildlogparseresultid"].ToString());

                    return ids;
                }
            }
        }

        #endregion

        #region REVISION

        Revision IDataPlugin.SaveRevision(Revision revision)
        {
            string insertQuery = @"
                INSERT INTO revision
                    (code, signature, sourceserverid, created, usr, files, description)
                VALUES
                    (@code, @signature, @sourceserverid, @created, @usr, @files, @description)
                RETURNING id";

            string updateQuery = @"                    
                UPDATE revision SET 
                    code = @code,
                    signature = @new_signature,
                    sourceserverid = @sourceserverid,
                    created = @created,
                    usr = @usr,
                    files = @files,
                    description = @description
                WHERE
                    id = @id 
                    AND signature = @signature";
            
            using (NpgsqlConnection connection = PostgresCommon.GetConnection(this.ContextPluginConfig))
            {
                if (string.IsNullOrEmpty(revision.Id))
                    revision.Id = PostgresCommon.InsertWithId<Revision>(this.ContextPluginConfig, insertQuery, revision, new ParameterMapper<Revision>(RevisionMapping.MapParameters), connection);
                else
                    PostgresCommon.Update<Revision>(this.ContextPluginConfig, updateQuery, revision, new ParameterMapper<Revision>(RevisionMapping.MapParameters), connection);

                return revision;
            }
        }

        Revision IDataPlugin.GetRevisionById(string id)
        {
            string query = @"
                SELECT 
                    *
                FROM 
                    revision
                WHERE 
                    id = @id";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("id", int.Parse(id));

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new RevisionConvert().ToCommon(reader);
            }
        }

        Revision IDataPlugin.GetRevisionByKey(string sourceServerId, string key)
        {
            string query = @"
                SELECT 
                    *
                FROM 
                    revision
                WHERE 
                    code = @id
                    AND sourceserverid=@sourceserverid";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("id", key);
                cmd.Parameters.AddWithValue("sourceserverid", int.Parse(sourceServerId));

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new RevisionConvert().ToCommon(reader);
            }
        }

        Revision IDataPlugin.GetNewestRevisionForBuild(string buildId)
        {
            string query = @"
                SELECT 
                    R.*
                FROM 
                    revision R 
                    JOIN buildinvolvement BI ON R.id = BI.revisionid
                WHERE 
                    BI.buildid = @buildid
                ORDER BY
                    R.created DESC
                LIMIT 
                    1";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("buildid", int.Parse(buildId));

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new RevisionConvert().ToCommon(reader);
            }
        }

        IEnumerable<Revision> IDataPlugin.GetRevisionByBuild(string buildId)
        {
            string query = @"
                SELECT 
                    R.*
                FROM 
                    revision R
                JOIN
                    buildinvolvement BI ON BI.revisionid = R.id
                WHERE 
                    BI.buildid = @buildid";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("buildId", int.Parse(buildId));

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new RevisionConvert().ToCommonList(reader);
            }
        }

        IEnumerable<Revision> IDataPlugin.GetRevisionsBySourceServer(string sourceServerId)
        {
            string query = @"
                SELECT 
                    *
                FROM 
                    revision
                WHERE 
                    sourceserverid = @sourceserverid";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("sourceserverid", int.Parse(sourceServerId));

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new RevisionConvert().ToCommonList(reader);
            }
        }

        bool IDataPlugin.DeleteRevision(Revision revision)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.Delete(conWrap.Connection(), this.ContextPluginConfig, "revision", "id", revision.Id);
        }

        #endregion

        #region SESSION

        Session IDataPlugin.SaveSession(Session session)
        {
            string insertQuery = @"
                INSERT INTO session
                    (ip, createdutc, useragent, userid)
                VALUES
                    (@ip, @createdutc, @useragent, @userid)
                RETURNING id";

            string updateQuery = @"                    
                UPDATE session SET 
                    ip = @ip, 
                    createdutc = @createdutc, 
                    useragent = @useragent, 
                    userid = @userid
                WHERE
                    id = @id";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            {
                if (string.IsNullOrEmpty(session.Id))
                    session.Id = PostgresCommon.InsertWithId<Session>(this.ContextPluginConfig, insertQuery, session, new ParameterMapper<Session>(SessionMapping.MapParameters), conWrap.Connection());
                else
                    PostgresCommon.Update<Session>(this.ContextPluginConfig, updateQuery, session, new ParameterMapper<Session>(SessionMapping.MapParameters), conWrap.Connection());

                return session;
            }
        }

        Session IDataPlugin.GetSessionById(string id)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.GetById<Session>(conWrap.Connection(), this.ContextPluginConfig, id, "session", new SessionConvert());
        }

        IEnumerable<Session> IDataPlugin.GetSessionByUserId(string userid)
        {
            string sql = @"SELECT * FROM session WHERE userid=@userid";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("userid", int.Parse(userid));

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new SessionConvert().ToCommonList(reader);
            }
        }

        bool IDataPlugin.DeleteSession(Session session)
        {
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
                return PostgresCommon.Delete(conWrap.Connection(), this.ContextPluginConfig, "session", "id", session.Id);
        }

        #endregion

        #region JOB DELTA

        Build IDataPlugin.GetLastJobDelta(string jobId)
        {
            string query = @"
                SELECT 
	                *
                FROM
	                build B
                WHERE
	                B.jobid=@jobid
	                AND B.status = (
		                -- get current passing or failing status of job 
		                SELECT 
			                status 
		                FROM 
			                build
		                WHERE
                            jobid = @jobid
			                AND (
                                status = @passing
			                    OR status = @failing
                            )
		                ORDER BY 
			                endedutc DESC
		                LIMIT 1
	                ) AND b.endedutc > (
		                -- get endutc of last build of previous status
		                SELECT 
			                endedutc 
		                FROM 
			                build
		                WHERE 
			                NOT endedutc IS NULL
                            AND jobid = @jobid
			                AND (
                                status = @passing
			                    OR status = @failing
                            )
			                AND NOT status = (
		                        -- get current passing or failing status of job 
		                        SELECT 
			                        status 
		                        FROM 
			                        build
		                        WHERE 
                                    jobid = @jobid
			                        AND (
                                        status = @passing
			                            OR status = @failing
                                    )
		                        ORDER BY 
			                        endedutc DESC
		                        LIMIT 1)
		                ORDER BY 
			                endedutc DESC
		                LIMIT 1)
                ORDER BY 
	                endedutc ASC
                LIMIT 1";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conWrap.Connection()))
            {
                cmd.Parameters.AddWithValue("jobid", int.Parse(jobId));
                cmd.Parameters.AddWithValue("passing", (int)BuildStatus.Passed);
                cmd.Parameters.AddWithValue("failing", (int)BuildStatus.Failed);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return new BuildConvert().ToCommon(reader);
            }
        }

        #endregion

        #region CONFIGURATIONSTATE

        int IDataPlugin.ClearAllTables() 
        {
            return PostgresCommon.ClearAllTables(this.ContextPluginConfig);
        }

        ConfigurationState IDataPlugin.AddConfigurationState(ConfigurationState configurationState)
        {
            string insertQuery = @"
                INSERT INTO configurationstate
                    (createdutc, hash, content)
                VALUES
                    (@createdutc, @hash, @content)
                RETURNING id";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            {
                configurationState.Id = PostgresCommon.InsertWithId<ConfigurationState>(this.ContextPluginConfig, insertQuery, configurationState, new ParameterMapper<ConfigurationState>(ConfigurationStateMapping.MapParameters), conWrap.Connection());
                return configurationState;
            }
        }

        ConfigurationState IDataPlugin.GetLatestConfigurationState()
        {
            string query = @"
                SELECT 
                    *
                FROM
                    configurationstate
                ORDER BY
                    createdutc DESC
                LIMIT 
                    1";

            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conWrap.Connection()))
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                return new ConfigurationStateConvert().ToCommon(reader);
        }

        PageableData<ConfigurationState> IDataPlugin.PageConfigurationStates(int index, int pageSize)
        {
            string pageQuery = @"
                SELECT
                    *
                FROM
                    configurationstate
                ORDER BY
                    createdutc DESC";

            string countQuery = @"
                SELECT 
                    COUNT(DISTINCT id )
                FROM 
                    configurationstate";

            long virtualItemCount = 0;
            IEnumerable<ConfigurationState> configurationStates = null;
            using (PostgresConnectionWrapper conWrap = new PostgresConnectionWrapper(this))
            {
                // get main records
                using (NpgsqlCommand cmd = new NpgsqlCommand(pageQuery, conWrap.Connection()))
                {
                    cmd.Parameters.AddWithValue("index", index * pageSize);
                    cmd.Parameters.AddWithValue("pagesize", pageSize);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        configurationStates = new ConfigurationStateConvert().ToCommonList(reader);
                }

                // get count of total records possible
                using (NpgsqlCommand cmd = new NpgsqlCommand(countQuery, conWrap.Connection()))
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    virtualItemCount = reader.GetInt64(0);
                }

                return new PageableData<ConfigurationState>(configurationStates, index, pageSize, virtualItemCount);
            }
        }

        #endregion
    }
}
