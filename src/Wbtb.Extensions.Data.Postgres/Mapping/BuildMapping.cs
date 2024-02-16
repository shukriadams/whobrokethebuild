using Npgsql;
using System;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class BuildMapping 
    {
        public static void MapParameters(Core.Common.Build record, NpgsqlParameterCollection queryParameters)
        {
            if (!string.IsNullOrEmpty(record.Id))
                queryParameters.AddWithValue("id", int.Parse(record.Id));

            queryParameters.AddWithValue("signature", record.Signature);
            queryParameters.AddWithValue("key", record.Key);
            queryParameters.AddWithValue("uniquepublickey", record.UniquePublicKey);
            queryParameters.AddWithValue("logfetched", record.LogFetched);
            queryParameters.AddWithValue("endedutc", record.EndedUtc == null ? (object)DBNull.Value : record.EndedUtc.Value);
            queryParameters.AddWithValue("hostname", string.IsNullOrEmpty(record.Hostname) ? (object)DBNull.Value : record.Hostname);
            queryParameters.AddWithValue("jobid", int.Parse(record.JobId));
            queryParameters.AddWithValue("incidentbuildid", string.IsNullOrEmpty(record.IncidentBuildId) ? (object)DBNull.Value : int.Parse(record.IncidentBuildId));
            queryParameters.AddWithValue("startedutc", record.StartedUtc);
            queryParameters.AddWithValue("status", (int)record.Status);
            queryParameters.AddWithValue("revisionInBuildLog", string.IsNullOrEmpty(record.RevisionInBuildLog) ? (object)DBNull.Value : record.RevisionInBuildLog);
        }
    }
}
