using Npgsql;

namespace Wbtb.Extensions.Data.Postgres
{
    public class SessionMapping
    {
        public static void MapParameters(Core.Common.Session record, NpgsqlParameterCollection queryParameters)
        {
            if (!string.IsNullOrEmpty(record.Id))
                queryParameters.AddWithValue("id", int.Parse(record.Id));

            queryParameters.AddWithValue("createdutc", record.CreatedUtc);
            queryParameters.AddWithValue("ip", record.IP.ToString());
            queryParameters.AddWithValue("useragent", record.UserAgent);
            queryParameters.AddWithValue("userid", int.Parse(record.UserId));
        }
    }
}
