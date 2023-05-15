using Npgsql;
using System;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class BuildMapping 
    {
        public static void MapParameters(Core.Common.Build record, NpgsqlParameterCollection collection)
        {
            if (!string.IsNullOrEmpty(record.Id))
                collection.AddWithValue("id", int.Parse(record.Id));

            collection.AddWithValue("signature", record.Signature);
            collection.AddWithValue("identifier", record.Identifier);
            collection.AddWithValue("logpath", string.IsNullOrEmpty(record.LogPath) ? (object)DBNull.Value : record.LogPath);
            collection.AddWithValue("endedutc", record.EndedUtc == null ? (object)DBNull.Value : record.EndedUtc.Value);
            collection.AddWithValue("hostname", string.IsNullOrEmpty(record.Hostname) ? (object)DBNull.Value : record.Hostname);
            collection.AddWithValue("jobid", int.Parse(record.JobId));
            collection.AddWithValue("incidentbuildid", string.IsNullOrEmpty(record.IncidentBuildId) ? (object)DBNull.Value : int.Parse(record.IncidentBuildId));
            collection.AddWithValue("startedutc", record.StartedUtc);
            collection.AddWithValue("status", (int)record.Status);
            collection.AddWithValue("triggeringcodechange", string.IsNullOrEmpty(record.TriggeringCodeChange) ? (object)DBNull.Value : record.TriggeringCodeChange);
            collection.AddWithValue("triggeringtype", string.IsNullOrEmpty(record.TriggeringType) ? (object)DBNull.Value : record.TriggeringType);
        }
    }
}
