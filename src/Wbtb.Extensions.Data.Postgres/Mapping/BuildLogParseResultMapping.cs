using Npgsql;
using System;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class BuildLogParseResultMapping 
    {
        public static void MapParameters(Core.Common.BuildLogParseResult record, NpgsqlParameterCollection queryParameters)
        {
            if (!string.IsNullOrEmpty(record.Id))
                queryParameters.AddWithValue("id", int.Parse(record.Id));

            queryParameters.AddWithValue("signature", record.Signature);
            queryParameters.AddWithValue("buildid", int.Parse(record.BuildId));
            queryParameters.AddWithValue("parsedcontent", string.IsNullOrEmpty(record.ParsedContent) ? (object)DBNull.Value : record.ParsedContent);
            queryParameters.AddWithValue("logparserplugin", record.LogParserPlugin);
        }
    }
}