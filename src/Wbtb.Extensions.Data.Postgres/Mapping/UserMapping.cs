using Npgsql;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class UserMapping 
    {
        public static void MapParameters(Core.Common.User record, NpgsqlParameterCollection collection)
        {
            if (!string.IsNullOrEmpty(record.Id))
                collection.AddWithValue("id", int.Parse(record.Id));

            collection.AddWithValue("key", record.Key);
        }
    }
}
