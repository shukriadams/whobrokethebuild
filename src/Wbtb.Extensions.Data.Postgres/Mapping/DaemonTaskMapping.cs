using Npgsql;
using System;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class DaemonTaskMapping
    {
        public static void MapParameters(Core.Common.DaemonTask record, NpgsqlParameterCollection collection)
        {
            if (!string.IsNullOrEmpty(record.Id))
                collection.AddWithValue("id", int.Parse(record.Id));

            collection.AddWithValue("signature", record.Signature);
            collection.AddWithValue("buildid", record.BuildId);
            collection.AddWithValue("taskkey", record.TaskKey);
            collection.AddWithValue("src", record.Src);
            collection.AddWithValue("buildinvolvevmentid", record.BuildInvolvementId == null ? (object)DBNull.Value : record.BuildInvolvementId);
            collection.AddWithValue("createdutc", record.CreatedUtc);
            collection.AddWithValue("result", record.Result == null ? (object)DBNull.Value : record.Result);
            collection.AddWithValue("processedutc", record.ProcessedUtc == null ? (object)DBNull.Value : record.ProcessedUtc.Value);
            collection.AddWithValue("passed", record.HasPassed == null ? (object)DBNull.Value : record.HasPassed.Value);
        }
    }
}
