using Npgsql;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class BuildLogParseResultConvert : IRecordConverter<BuildLogParseResult>
    {
        private BuildLogParseResult ToCommonSingle(NpgsqlDataReader reader)
        {
            return new BuildLogParseResult
            {
                Id = reader["id"].ToString(),
                ParsedContent = reader["parsedcontent"].ToString(),
                BuildId = reader["buildid"].ToString(),
                LogParserPlugin = reader["logparserplugin"].ToString()
            };
        }

        public BuildLogParseResult ToCommon(NpgsqlDataReader reader)
        {
            if (!reader.HasRows)
                return null;

            reader.Read();
            return ToCommonSingle(reader);
        }

        public IEnumerable<BuildLogParseResult> ToCommonList(NpgsqlDataReader reader)
        {
            IList<BuildLogParseResult> list = new List<BuildLogParseResult>();
            while (reader.Read())
                list.Add(this.ToCommonSingle(reader));

            return list;
        }
    }
}
