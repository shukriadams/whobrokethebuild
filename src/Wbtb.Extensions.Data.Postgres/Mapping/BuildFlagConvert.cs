using Npgsql;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    public class BuildFlagConvert : IRecordConverter<BuildFlag>
    {
        private BuildFlag ToCommonSingle(NpgsqlDataReader reader)
        {
            return new BuildFlag
            {
                Id = reader["id"].ToString(),
                BuildId = reader["buildid"].ToString(),
                Flag = (BuildFlags)reader["flag"],
                Ignored = reader["ignored"] == DBNull.Value ? false : bool.Parse(reader["ignored"].ToString()),
                CreatedUtc = DateTime.Parse(reader["createdutc"].ToString()),
                Description = reader["description"].ToString()
            };
        }

        public BuildFlag ToCommon(NpgsqlDataReader reader)
        {
            if (!reader.HasRows)
                return null;

            reader.Read();
            return ToCommonSingle(reader);
        }

        public IEnumerable<BuildFlag> ToCommonList(NpgsqlDataReader reader)
        {
            IList<BuildFlag> list = new List<BuildFlag>();
            while (reader.Read())
                list.Add(this.ToCommonSingle(reader));

            return list;
        }
    }
}
