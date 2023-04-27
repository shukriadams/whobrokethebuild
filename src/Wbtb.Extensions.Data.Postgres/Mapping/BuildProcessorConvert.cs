using Npgsql;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    public class BuildProcessorConvert : IRecordConverter<BuildProcessor>
    {
        private BuildProcessor ToCommonSingle(NpgsqlDataReader reader)
        {
            return new BuildProcessor
            {
                Id = reader["id"].ToString(),
                Signature = reader["signature"].ToString(),
                BuildId = reader["buildid"].ToString(),
                ProcessorKey = reader["processor"].ToString(),
                Status = (BuildProcessorStatus)reader["status"]
            };
        }

        public BuildProcessor ToCommon(NpgsqlDataReader reader)
        {
            if (!reader.HasRows)
                return null;

            reader.Read();
            return ToCommonSingle(reader);
        }

        public IEnumerable<BuildProcessor> ToCommonList(NpgsqlDataReader reader)
        {
            IList<BuildProcessor> list = new List<BuildProcessor>();
            while (reader.Read())
                list.Add(this.ToCommonSingle(reader));

            return list;
        }
    }
}
