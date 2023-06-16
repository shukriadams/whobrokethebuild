using Npgsql;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class BuildServerConvert : IRecordConverter<BuildServer>
    {
        private readonly Configuration _config;

        public BuildServerConvert(Configuration config)
        {
            _config = config;
        }

        private BuildServer ToCommonSingle(NpgsqlDataReader reader)
        {
            BuildServer buildserver = new BuildServer
            {
                Id = reader["id"].ToString(),
                Key = reader["key"].ToString()
            };

            // fill in rest of values from config
            BuildServer config = _config.BuildServers.SingleOrDefault(r => r.Key == buildserver.Key);
            if (config != null)
            { 
                buildserver.Config = config.Config;
                buildserver.Name = config.Name;
                buildserver.Plugin = config.Plugin;
                buildserver.Description = config.Description;
                buildserver.Enable = config.Enable;
                buildserver.Jobs = config.Jobs;
                buildserver.Name = config.Name;
                buildserver.ImportCount = config.ImportCount;
                buildserver.ServerType = config.ServerType;
            }

            return buildserver;
        }

        public BuildServer ToCommon(NpgsqlDataReader reader)
        {
            if (!reader.HasRows)
                return null;

            reader.Read();
            return ToCommonSingle(reader);
        }

        public IEnumerable<BuildServer> ToCommonList(NpgsqlDataReader reader)
        {
            IList<BuildServer> list = new List<BuildServer>();
            while (reader.Read())
                list.Add(this.ToCommonSingle(reader));

            return list;
        }
    }
}
