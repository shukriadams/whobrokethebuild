using Npgsql;
using System;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class JobMapping 
    {
        public static void MapRevisionParameters(Core.Common.Job record, NpgsqlParameterCollection queryParameters)
        {
            if (!string.IsNullOrEmpty(record.Id))
                queryParameters.AddWithValue("id", int.Parse(record.Id));

            queryParameters.AddWithValue("buildserverid", int.Parse(record.BuildServerId));
            queryParameters.AddWithValue("sourceserverId", string.IsNullOrEmpty(record.SourceServerId) ? (object)DBNull.Value : int.Parse(record.SourceServerId));
            queryParameters.AddWithValue("key", record.Key);
        }
    }
}
