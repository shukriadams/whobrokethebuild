using Npgsql;

namespace Wbtb.Extensions.Data.Postgres
{
    public class SessionMapping
    {
        public static void MapParameters(Core.Common.Session record, NpgsqlParameterCollection collection)
        {
            if (!string.IsNullOrEmpty(record.Id))
                collection.AddWithValue("id", int.Parse(record.Id));

            collection.AddWithValue("createdutc", record.CreatedUtc);
            collection.AddWithValue("ip", record.IP.ToString());
            collection.AddWithValue("useragent", record.UserAgent);
            collection.AddWithValue("userid", int.Parse(record.UserId));
        }
    }
}
