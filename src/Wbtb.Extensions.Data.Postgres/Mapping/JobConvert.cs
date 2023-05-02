using Npgsql;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class JobConvert : IRecordConverter<Job>
    {
        private readonly Config _config;

        internal JobConvert(Config config) 
        {
            _config = config;
        }
        
        private Job ToCommonSingle(NpgsqlDataReader reader)
        {
            Job job = new Job
            {
                Id = reader["id"].ToString(),
                BuildServerId = reader["buildserverid"].ToString(),
                SourceServerId = reader["sourceserverid"].ToString(),
                Key = reader["key"].ToString()
            };

            // fill in rest of values from config
            foreach (BuildServer buildServer in _config.BuildServers)
                foreach (Job config in buildServer.Jobs)
                { 
                    if (config.Key == job.Key)
                    {
                        job.Alert = config.Alert;
                        job.BuildServer = buildServer.Key;
                        job.Description = config.Description;
                        job.Enable = config.Enable;
                        job.HistoryLimit = config.HistoryLimit;
                        job.Image = config.Image;
                        job.ImportCount = config.ImportCount;
                        job.LogParserPlugins = config.LogParserPlugins;
                        job.Name = config.Name;
                        job.SourceServer = config.SourceServer;
                        job.RevisionAtBuildRegex = config.RevisionAtBuildRegex;
                        job.RevisionScrapeSpanBuilds = config.RevisionScrapeSpanBuilds;
                        job.OnBuildStart = config.OnBuildStart;
                        job.OnBuildEnd = config.OnBuildEnd;
                        job.OnFixed = config.OnFixed;
                        job.OnBroken= config.OnBroken;
                        job.OnLogAvailable = config.OnLogAvailable;

                        return job;
                    }
                }

            return job;
        }

        public Job ToCommon(NpgsqlDataReader reader)
        {
            if (!reader.HasRows)
                return null;

            reader.Read();
            return ToCommonSingle(reader);
        }

        public IEnumerable<Job> ToCommonList(NpgsqlDataReader reader)
        {
            IList<Job> list = new List<Job>();
            while (reader.Read())
                list.Add(this.ToCommonSingle(reader));

            return list;
        }
    }
}
