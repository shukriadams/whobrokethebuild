using Npgsql;
using System;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class BuildFlagMapping
    {
        public static void MapParameters(Core.Common.BuildFlag record, NpgsqlParameterCollection collection)
        {
            if (!string.IsNullOrEmpty(record.Id))
                collection.AddWithValue("id", int.Parse(record.Id));

            collection.AddWithValue("flag", (int)record.Flag);
            collection.AddWithValue("description", record.Description);
            collection.AddWithValue("buildid", int.Parse(record.BuildId));
            collection.AddWithValue("createdutc", record.CreatedUtc);
            collection.AddWithValue("ignored", record.Ignored ? record.Ignored : (object)DBNull.Value);
        }
    }
}
