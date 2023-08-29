using Npgsql;
using System;
using System.Linq;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class IncidentReportMapping
    {
        public static void MapParameters(Core.Common.IncidentReport record, NpgsqlParameterCollection queryParameters)
        {
            if (!string.IsNullOrEmpty(record.Id))
                queryParameters.AddWithValue("id", int.Parse(record.Id));

            queryParameters.AddWithValue("signature", record.Signature);
            queryParameters.AddWithValue("incidentid", int.Parse(record.IncidentId));
            queryParameters.AddWithValue("mutationid", int.Parse(record.MutationId));
            queryParameters.AddWithValue("description", record.IncidentId == null ? (object)DBNull.Value : record.Description);
            queryParameters.AddWithValue("implicatedrevisions", record.ImplicatedRevisions.Any() ? string.Join(",", record.ImplicatedRevisions) : (object)DBNull.Value);
            queryParameters.AddWithValue("createdutc", record.CreatedUtc);
            queryParameters.AddWithValue("processor", record.Processor);
            queryParameters.AddWithValue("status", record.Status);
            queryParameters.AddWithValue("summary", record.Summary);
        }
    }
}
