using Npgsql;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class DaemonTaskConvert : IRecordConverter<DaemonTask>
    {
        private DaemonTask ToCommonSingle(NpgsqlDataReader reader)
        {
            return new DaemonTask
            {
                Id = reader["id"].ToString(),
                Signature = reader["signature"].ToString(),
                BuildId = reader["buildid"].ToString(),
                Order = int.Parse(reader["ordr"].ToString()),
                BuildInvolvementId = reader["buildinvolvementid"] == DBNull.Value ? null : reader["buildinvolvementid"].ToString(),
                CreatedUtc = DateTime.Parse(reader["createdutc"].ToString()),
                ProcessedUtc = reader["processedutc"] == DBNull.Value ? null : DateTime.Parse(reader["processedutc"].ToString()),
                HasPassed = reader["passed"] == DBNull.Value ? null : bool.Parse(reader["passed"].ToString()),
                Result = reader["result"] == DBNull.Value ? null : reader["result"].ToString(),
                Args = reader["args"] == DBNull.Value ? null : reader["args"].ToString(),
                Src = reader["src"].ToString(),
                TaskKey = reader["taskkey"].ToString()
            };
        }

        public DaemonTask ToCommon(NpgsqlDataReader reader)
        {
            if (!reader.HasRows)
                return null;

            reader.Read();
            return ToCommonSingle(reader);
        }

        public IEnumerable<DaemonTask> ToCommonList(NpgsqlDataReader reader)
        {
            IList<DaemonTask> list = new List<DaemonTask>();
            while (reader.Read())
                list.Add(this.ToCommonSingle(reader));

            return list;
        }
    }
}
