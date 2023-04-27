using Npgsql;

namespace Wbtb.Extensions.Data.Postgres
{
    public class SourceServerMapping
    {
        public static void MapParameters(Core.Common.SourceServer record, NpgsqlParameterCollection collection)
        {
            if (!string.IsNullOrEmpty(record.Id))
                collection.AddWithValue("id", int.Parse(record.Id));

            collection.AddWithValue("key", record.Key);
        }
    }
}
