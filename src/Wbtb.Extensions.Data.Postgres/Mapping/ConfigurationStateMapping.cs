using Npgsql;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    public class ConfigurationStateMapping
    {
        public static void MapParameters(ConfigurationState record, NpgsqlParameterCollection queryParameters)
        {
            if (!string.IsNullOrEmpty(record.Id))
                queryParameters.AddWithValue("id", int.Parse(record.Id));
            
            queryParameters.AddWithValue("createdutc", record.CreatedUtc);
            queryParameters.AddWithValue("content", record.Content);
            queryParameters.AddWithValue("hash", record.Hash);
        }
    }
}
