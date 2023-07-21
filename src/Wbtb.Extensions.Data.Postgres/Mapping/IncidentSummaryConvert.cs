using Npgsql;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class IncidentSummaryConvert : IRecordConverter<IncidentSummary>
    {
        private IncidentSummary ToCommonSingle(NpgsqlDataReader reader)
        {
            return new IncidentSummary
            {
                Id = reader["id"].ToString(),
                Signature = reader["signature"].ToString(),
                IncidentId = reader["incidentid"].ToString(),
                MutationId = reader["mutationid"].ToString(),
                Description = reader["description"] == DBNull.Value ? null : reader["description"].ToString(),
                Processor = reader["processor"].ToString(),
                Status = reader["status"].ToString(),
                Summary = reader["summary"].ToString()
            };
        }

        public IncidentSummary ToCommon(NpgsqlDataReader reader)
        {
            if (!reader.HasRows)
                return null;

            reader.Read();
            return ToCommonSingle(reader);
        }

        public IEnumerable<IncidentSummary> ToCommonList(NpgsqlDataReader reader)
        {
            IList<IncidentSummary> list = new List<IncidentSummary>();
            while (reader.Read())
                list.Add(this.ToCommonSingle(reader));

            return list;
        }
    }
}
