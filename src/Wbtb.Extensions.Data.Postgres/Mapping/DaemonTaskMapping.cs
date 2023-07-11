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
            collection.AddWithValue("buildid", int.Parse(record.BuildId));
            collection.AddWithValue("ordr", record.Order);
            collection.AddWithValue("taskkey", record.TaskKey);
            collection.AddWithValue("src", record.Src);
            collection.AddWithValue("buildinvolvementid", record.BuildInvolvementId == null ? (object)DBNull.Value : int.Parse(record.BuildInvolvementId));
            collection.AddWithValue("createdutc", record.CreatedUtc);
            collection.AddWithValue("result", record.Result == null ? (object)DBNull.Value : record.Result);
            collection.AddWithValue("args", record.Args == null ? (object)DBNull.Value : record.Args);
            collection.AddWithValue("processedutc", record.ProcessedUtc == null ? (object)DBNull.Value : record.ProcessedUtc.Value);
            collection.AddWithValue("passed", record.HasPassed == null ? (object)DBNull.Value : record.HasPassed.Value);
        }
    }
}
