using Npgsql;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    public class SourceServerConvert : IRecordConverter<SourceServer>
    {
        private readonly Configuration _config;

        internal SourceServerConvert(Configuration config) 
        {
            _config = config;
        }

        private SourceServer ToCommonSingle(NpgsqlDataReader reader)
        {
            SourceServer sourceServer = new SourceServer
            {
                Id = reader["id"].ToString(),
                Key = reader["key"].ToString()
            };

            // fill in rest of values from config
            SourceServer config = _config.SourceServers.SingleOrDefault(r => r.Key == sourceServer.Key);
            if (config != null)
            {
                sourceServer.Config = config.Config;
                sourceServer.Name = config.Name;
                sourceServer.Plugin = config.Plugin;
                sourceServer.Description = config.Description;
                sourceServer.Enable = config.Enable;
                sourceServer.Name = config.Name;
                sourceServer.ServerType = config.ServerType;
            }

            return sourceServer;
        }

        public SourceServer ToCommon(NpgsqlDataReader reader)
        {
            if (!reader.HasRows)
                return null;

            reader.Read();
            return ToCommonSingle(reader);
        }


        public IEnumerable<SourceServer> ToCommonList(NpgsqlDataReader reader)
        {
            IList<SourceServer> list = new List<SourceServer>();
            while (reader.Read())
                list.Add(this.ToCommonSingle(reader));

            return list;
        }
    }
}
