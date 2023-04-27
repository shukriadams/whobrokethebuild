using Npgsql;
using System;
using System.Collections.Generic;

namespace Wbtb.Extensions.Data.Postgres
{
    public class SessionConvert : IRecordConverter<Core.Common.Session>
    {
        private Core.Common.Session ToCommonSingle(NpgsqlDataReader reader)
        {
            return new Core.Common.Session
            {
                Id = reader["id"].ToString(),
                CreatedUtc = DateTime.Parse(reader["createdutc"].ToString()),
                IP = reader["ip"].ToString(),
                UserAgent = reader["useragent"].ToString(),
                UserId = reader["userid"].ToString()
            };
        }

        public Core.Common.Session ToCommon(NpgsqlDataReader reader)
        {
            if (!reader.HasRows)
                return null;

            reader.Read();
            return ToCommonSingle(reader);
        }

        public IEnumerable<Core.Common.Session> ToCommonList(NpgsqlDataReader reader)
        {
            IList<Core.Common.Session> list = new List<Core.Common.Session>();
            while (reader.Read())
                list.Add(this.ToCommonSingle(reader));

            return list;
        }
    }
}
