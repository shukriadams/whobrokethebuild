using Npgsql;

namespace Wbtb.Extensions.Data.Postgres
{
    public class SourceServerMapping
    {
        public static void MapParameters(Core.Common.SourceServer record, NpgsqlParameterCollection queryParameters)
        {
            if (!string.IsNullOrEmpty(record.Id))
                queryParameters.AddWithValue("id", int.Parse(record.Id));

            queryParameters.AddWithValue("key", record.Key);
        }
    }
}
