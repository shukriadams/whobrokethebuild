using Npgsql;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class RevisionConvert : IRecordConverter<Revision>
    {
        private Revision ToCommonSingle(NpgsqlDataReader reader)
        {
            return new Revision
            {
                Id = reader["id"].ToString(),
                Signature = reader["signature"].ToString(),
                Code = reader["code"].ToString(),
                Created = DateTime.Parse(reader["created"].ToString()),
                Description = reader["description"].ToString(),
                Files = JsonConvert.DeserializeObject<IEnumerable<RevisionFile>>(reader["files"].ToString()),
                SourceServerId = reader["sourceserverid"].ToString(),
                User = reader["usr"].ToString(),
            };
        }

        public Revision ToCommon(NpgsqlDataReader reader)
        {
            if (!reader.HasRows)
                return null;

            reader.Read();
            return ToCommonSingle(reader);
        }

        public IEnumerable<Revision> ToCommonList(NpgsqlDataReader reader)
        {
            IList<Revision> list = new List<Revision>();
            while (reader.Read())
                list.Add(this.ToCommonSingle(reader));

            return list;
        }
    }
}
