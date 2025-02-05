using Npgsql;
using System;

namespace Wbtb.Extensions.Data.Postgres
{
    public class WorkProcessMapping
    {
        public static void MapParameters(Core.Common.WorkProcess record, NpgsqlParameterCollection queryParameters)
        {
            if (!string.IsNullOrEmpty(record.Id))
                queryParameters.AddWithValue("id", int.Parse(record.Id));

            queryParameters.AddWithValue("signature", record.Content);
            queryParameters.AddWithValue("content", string.IsNullOrEmpty(record.Content) ? (object)DBNull.Value : record.Content);
            queryParameters.AddWithValue("key", record.Key);
            queryParameters.AddWithValue("category", record.Category);
            queryParameters.AddWithValue("createdutc", record.CreatedUtc);
            queryParameters.AddWithValue("lifespan", record.Lifespan.HasValue ? record.Lifespan.Value : (object)DBNull.Value);
            queryParameters.AddWithValue("keptaliveutc", record.Lifespan.HasValue ? record.KeptAliveUtc.Value : (object)DBNull.Value);
        }
    }
}
