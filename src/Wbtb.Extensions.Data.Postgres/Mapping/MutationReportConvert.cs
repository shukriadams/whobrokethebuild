using Npgsql;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class MutationReportConvert : IRecordConverter<MutationReport>
    {
        private MutationReport ToCommonSingle(NpgsqlDataReader reader)
        {
            return new MutationReport
            {
                Id = reader["id"].ToString(),
                Signature = reader["signature"].ToString(),
                IncidentId = reader["incidentid"].ToString(),
                BuildId = reader["buildid"].ToString(),
                MutationId = reader["mutationid"].ToString(),
                Description = reader["description"] == DBNull.Value ? null : reader["description"].ToString(),
                MutationHash = reader["mutationhash"] == DBNull.Value ? null : reader["mutationhash"].ToString(),
                ImplicatedRevisions = reader["implicatedrevisions"] == DBNull.Value ? new string[] { } : reader["implicatedrevisions"].ToString().Split(),
                Processor = reader["processor"].ToString(),
                Status = reader["status"].ToString(),
                Summary = reader["summary"].ToString()
            };
        }

        public MutationReport ToCommon(NpgsqlDataReader reader)
        {
            if (!reader.HasRows)
                return null;

            reader.Read();
            return ToCommonSingle(reader);
        }

        public IEnumerable<MutationReport> ToCommonList(NpgsqlDataReader reader)
        {
            IList<MutationReport> list = new List<MutationReport>();
            while (reader.Read())
                list.Add(this.ToCommonSingle(reader));

            return list;
        }
    }
}
