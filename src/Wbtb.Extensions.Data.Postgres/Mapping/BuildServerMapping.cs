using Npgsql;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    public class BuildServerMapping 
    {
        public static void MapParameters(BuildServer record, NpgsqlParameterCollection collection)
        {
            if (!string.IsNullOrEmpty(record.Id))
                collection.AddWithValue("id", int.Parse(record.Id));

            collection.AddWithValue("key", record.Key);
        }
    }
}
