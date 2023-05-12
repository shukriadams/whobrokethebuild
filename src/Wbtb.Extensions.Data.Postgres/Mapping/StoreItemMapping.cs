using Npgsql;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class StoreItemMapping
    {
        public static void MapParameters(Core.Common.StoreItem record, NpgsqlParameterCollection collection)
        {
            if (!string.IsNullOrEmpty(record.Id))
                collection.AddWithValue("id", int.Parse(record.Id));

            collection.AddWithValue("key", record.Key);
            collection.AddWithValue("plugin", record.Plugin);
            collection.AddWithValue("content", record.Content);
        }
    }
}
