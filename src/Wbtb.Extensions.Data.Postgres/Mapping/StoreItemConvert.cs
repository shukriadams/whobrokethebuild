using Npgsql;
using System.Collections.Generic;

namespace Wbtb.Extensions.Data.Postgres
{

    internal class StoreItemConvert : IRecordConverter<Core.Common.StoreItem>
    {
        private Core.Common.StoreItem ToCommonSingle(NpgsqlDataReader reader)
        {
            return new Core.Common.StoreItem
            {
                Id = reader["id"].ToString(),
                Key = reader["signature"].ToString(),
                Plugin = reader["plugin"].ToString(),
                Content = reader["content"].ToString(),
            };
        }

        public Core.Common.StoreItem ToCommon(NpgsqlDataReader reader)
        {
            if (!reader.HasRows)
                return null;

            reader.Read();
            return ToCommonSingle(reader);
        }

        public IEnumerable<Core.Common.StoreItem> ToCommonList(NpgsqlDataReader reader)
        {
            IList<Core.Common.StoreItem> list = new List<Core.Common.StoreItem>();
            while (reader.Read())
                list.Add(this.ToCommonSingle(reader));

            return list;
        }
    }
}
