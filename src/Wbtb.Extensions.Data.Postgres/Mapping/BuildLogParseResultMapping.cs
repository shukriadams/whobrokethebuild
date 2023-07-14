using Npgsql;
using System;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class BuildLogParseResultMapping 
    {
        public static void MapParameters(Core.Common.BuildLogParseResult record, NpgsqlParameterCollection collection)
        {
            if (!string.IsNullOrEmpty(record.Id))
                collection.AddWithValue("id", int.Parse(record.Id));

            collection.AddWithValue("signature", record.Signature);
            collection.AddWithValue("buildid", int.Parse(record.BuildId));
            collection.AddWithValue("parsedcontent", string.IsNullOrEmpty(record.ParsedContent) ? (object)DBNull.Value : record.ParsedContent);
            collection.AddWithValue("logparserplugin", record.LogParserPlugin);
        }
    }
}