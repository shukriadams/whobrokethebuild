using Npgsql;
using System;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class StoreItemMapping
    {
        public static void MapParameters(Core.Common.StoreItem record, NpgsqlParameterCollection queryParameters)
        {
            if (!string.IsNullOrEmpty(record.Id))
                queryParameters.AddWithValue("id", int.Parse(record.Id));

            queryParameters.AddWithValue("key", record.Key);
            queryParameters.AddWithValue("plugin", record.Plugin);
            queryParameters.AddWithValue("content", string.IsNullOrEmpty(record.Content) ? (object)DBNull.Value : record.Content);
        }
    }
}
