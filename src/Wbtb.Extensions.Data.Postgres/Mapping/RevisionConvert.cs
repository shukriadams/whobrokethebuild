using Npgsql;
using System;
using System.Collections.Generic;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class RevisionConvert : IRecordConverter<Core.Common.Revision>
    {
        private Core.Common.Revision ToCommonSingle(NpgsqlDataReader reader)
        {
            string files = reader["files"].ToString();
            if (string.IsNullOrEmpty(files))
                files = "";

            IEnumerable<string> filesList = files.Split(",", StringSplitOptions.RemoveEmptyEntries);

            return new Core.Common.Revision
            {
                Id = reader["id"].ToString(),
                Signature = reader["signature"].ToString(),
                Code = reader["code"].ToString(),
                Created = DateTime.Parse(reader["created"].ToString()),
                Description = reader["description"].ToString(),
                Files = filesList,
                SourceServerId = reader["sourceserverid"].ToString(),
                User = reader["usr"].ToString(),
            };
        }

        public Core.Common.Revision ToCommon(NpgsqlDataReader reader)
        {
            if (!reader.HasRows)
                return null;

            reader.Read();
            return ToCommonSingle(reader);
        }

        public IEnumerable<Core.Common.Revision> ToCommonList(NpgsqlDataReader reader)
        {
            IList<Core.Common.Revision> list = new List<Core.Common.Revision>();
            while (reader.Read())
                list.Add(this.ToCommonSingle(reader));

            return list;
        }
    }
}
