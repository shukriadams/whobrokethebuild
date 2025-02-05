using Npgsql;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    public class WorkProcessConvert : IRecordConverter<WorkProcess>
    {
        private WorkProcess ToCommonSingle(NpgsqlDataReader reader)
        {
            return new WorkProcess
            {
                Id = reader["id"].ToString(),
                Key = reader["key"].ToString(),
                CreatedUtc = DateTime.Parse(reader["createdutc"].ToString()),
                KeptAliveUtc = reader["keptaliveutc"] == DBNull.Value ? (DateTime?)null : DateTime.Parse(reader["keptaliveutc"].ToString()),
                Category = reader["category"].ToString(),
                Content = reader["content"] == DBNull.Value ? null : reader["content"].ToString(),
                Lifespan = reader["lifespan"] == DBNull.Value ? (int?)null : int.Parse(reader["lifespan"].ToString())
            };
        }

        public WorkProcess ToCommon(NpgsqlDataReader reader)
        {
            if (!reader.HasRows)
                return null;

            reader.Read();
            return ToCommonSingle(reader);
        }

        public IEnumerable<WorkProcess> ToCommonList(NpgsqlDataReader reader)
        {
            IList<WorkProcess> list = new List<WorkProcess>();
            while (reader.Read())
                list.Add(this.ToCommonSingle(reader));

            return list;
        }
    }
}
