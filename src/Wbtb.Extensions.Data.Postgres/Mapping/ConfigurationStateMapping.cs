using Npgsql;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    public class ConfigurationStateMapping
    {
        public static void MapParameters(ConfigurationState record, NpgsqlParameterCollection collection)
        {
            if (!string.IsNullOrEmpty(record.Id))
                collection.AddWithValue("id", int.Parse(record.Id));
            
            collection.AddWithValue("createdutc", record.CreatedUtc);
            collection.AddWithValue("content", record.Content);
            collection.AddWithValue("hash", record.Hash);
        }
    }
}
