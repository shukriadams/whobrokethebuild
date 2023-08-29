using Npgsql;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class IncidentReportConvert : IRecordConverter<IncidentReport>
    {
        private IncidentReport ToCommonSingle(NpgsqlDataReader reader)
        {
            return new IncidentReport
            {
                Id = reader["id"].ToString(),
                Signature = reader["signature"].ToString(),
                IncidentId = reader["incidentid"].ToString(),
                MutationId = reader["mutationid"].ToString(),
                Description = reader["description"] == DBNull.Value ? null : reader["description"].ToString(),
                ImplicatedRevisions = reader["implicatedrevisions"] == DBNull.Value ? new string[] { } : reader["implicatedrevisions"].ToString().Split(),
                Processor = reader["processor"].ToString(),
                Status = reader["status"].ToString(),
                Summary = reader["summary"].ToString()
            };
        }

        public IncidentReport ToCommon(NpgsqlDataReader reader)
        {
            if (!reader.HasRows)
                return null;

            reader.Read();
            return ToCommonSingle(reader);
        }

        public IEnumerable<IncidentReport> ToCommonList(NpgsqlDataReader reader)
        {
            IList<IncidentReport> list = new List<IncidentReport>();
            while (reader.Read())
                list.Add(this.ToCommonSingle(reader));

            return list;
        }
    }
}
