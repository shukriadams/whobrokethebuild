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
            queryParameters.AddWithValue("identifier", record.Identifier);
            queryParameters.AddWithValue("logpath", string.IsNullOrEmpty(record.LogPath) ? (object)DBNull.Value : record.LogPath);
            queryParameters.AddWithValue("endedutc", record.EndedUtc == null ? (object)DBNull.Value : record.EndedUtc.Value);
            queryParameters.AddWithValue("hostname", string.IsNullOrEmpty(record.Hostname) ? (object)DBNull.Value : record.Hostname);
            queryParameters.AddWithValue("jobid", int.Parse(record.JobId));
            queryParameters.AddWithValue("incidentbuildid", string.IsNullOrEmpty(record.IncidentBuildId) ? (object)DBNull.Value : int.Parse(record.IncidentBuildId));
            queryParameters.AddWithValue("startedutc", record.StartedUtc);
            queryParameters.AddWithValue("status", (int)record.Status);
            queryParameters.AddWithValue("triggeringcodechange", string.IsNullOrEmpty(record.TriggeringCodeChange) ? (object)DBNull.Value : record.TriggeringCodeChange);
            queryParameters.AddWithValue("triggeringtype", string.IsNullOrEmpty(record.TriggeringType) ? (object)DBNull.Value : record.TriggeringType);
        }
    }
}
