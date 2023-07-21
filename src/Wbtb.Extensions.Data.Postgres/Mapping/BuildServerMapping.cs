using Npgsql;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    public class BuildServerMapping 
    {
        public static void MapParameters(BuildServer record, NpgsqlParameterCollection queryParameters)
        {
            if (!string.IsNullOrEmpty(record.Id))
                queryParameters.AddWithValue("id", int.Parse(record.Id));

            queryParameters.AddWithValue("key", record.Key);
        }
    }
}
