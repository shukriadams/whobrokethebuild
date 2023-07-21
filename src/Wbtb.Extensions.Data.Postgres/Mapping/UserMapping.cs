using Npgsql;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class UserMapping 
    {
        public static void MapParameters(Core.Common.User record, NpgsqlParameterCollection queryParameters)
        {
            if (!string.IsNullOrEmpty(record.Id))
                queryParameters.AddWithValue("id", int.Parse(record.Id));

            queryParameters.AddWithValue("key", record.Key);
        }
    }
}
