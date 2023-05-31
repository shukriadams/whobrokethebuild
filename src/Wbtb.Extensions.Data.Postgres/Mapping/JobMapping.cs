using Npgsql;
using System;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class JobMapping 
    {
        public static void MapRevisionParameters(Core.Common.Job record, NpgsqlParameterCollection collection)
        {
            if (!string.IsNullOrEmpty(record.Id))
                collection.AddWithValue("id", int.Parse(record.Id));

            collection.AddWithValue("buildserverid", int.Parse(record.BuildServerId));
            collection.AddWithValue("sourceserverId", string.IsNullOrEmpty(record.SourceServerId) ? (object)DBNull.Value : int.Parse(record.SourceServerId));
            collection.AddWithValue("key", record.Key);
        }
    }
}
