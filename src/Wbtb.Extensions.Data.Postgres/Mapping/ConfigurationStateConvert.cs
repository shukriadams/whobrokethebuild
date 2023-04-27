using Npgsql;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    public class ConfigurationStateConvert : IRecordConverter<ConfigurationState>
    {
        private ConfigurationState ToCommonSingle(NpgsqlDataReader reader)
        {
            return new ConfigurationState
            {
                Id = reader["id"].ToString(),
                CreatedUtc = DateTime.Parse(reader["createdutc"].ToString()),
                Hash = reader["hash"].ToString(),
                Content = reader["content"].ToString()
            };
        }

        public ConfigurationState ToCommon(NpgsqlDataReader reader)
        {
            if (!reader.HasRows)
                return null;

            reader.Read();
            return ToCommonSingle(reader);
        }

        public IEnumerable<ConfigurationState> ToCommonList(NpgsqlDataReader reader)
        {
            IList<ConfigurationState> list = new List<ConfigurationState>();
            while (reader.Read())
                list.Add(this.ToCommonSingle(reader));

            return list;
        }
    }
}
