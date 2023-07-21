using Npgsql;
using System;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class DaemonTaskMapping
    {
        public static void MapParameters(Core.Common.DaemonTask record, NpgsqlParameterCollection queryParameters)
        {
            if (!string.IsNullOrEmpty(record.Id))
                queryParameters.AddWithValue("id", int.Parse(record.Id));

            queryParameters.AddWithValue("signature", record.Signature);
            queryParameters.AddWithValue("buildid", int.Parse(record.BuildId));
            queryParameters.AddWithValue("stage", record.Stage);
            queryParameters.AddWithValue("src", record.Src);
            queryParameters.AddWithValue("buildinvolvementid", record.BuildInvolvementId == null ? (object)DBNull.Value : int.Parse(record.BuildInvolvementId));
            queryParameters.AddWithValue("createdutc", record.CreatedUtc);
            queryParameters.AddWithValue("result", record.Result == null ? (object)DBNull.Value : record.Result);
            queryParameters.AddWithValue("args", record.Args == null ? (object)DBNull.Value : record.Args);
            queryParameters.AddWithValue("processedutc", record.ProcessedUtc == null ? (object)DBNull.Value : record.ProcessedUtc.Value);
            queryParameters.AddWithValue("passed", record.HasPassed == null ? (object)DBNull.Value : record.HasPassed.Value);
        }
    }
}
