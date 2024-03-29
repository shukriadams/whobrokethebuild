﻿using Npgsql;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class UserConvert : IRecordConverter<User>
    {
        private readonly Configuration _config;

        internal UserConvert(Configuration config) 
        { 
            _config = config;
        }

        private User ToCommonSingle(NpgsqlDataReader reader)
        {
            User user = new User
            {
                Id = reader["id"].ToString(),
                Key = reader["key"].ToString()
            };

            // fill in rest of values from config
            User config = _config.Users.SingleOrDefault(r => r.Key == user.Key);
            if (config != null)
            {
                user.Name = config.Name;
                user.Message = config.Message;
                user.Description = config.Description;
                user.Enable = config.Enable;
                user.Image = config.Image;
                user.SourceServerIdentities = config.SourceServerIdentities;
                user.AuthPlugin = config.AuthPlugin;
            }

            return user;
        }

        public User ToCommon(NpgsqlDataReader reader)
        {
            if (!reader.HasRows)
                return null;

            reader.Read();
            return ToCommonSingle(reader);
        }

        public IEnumerable<User> ToCommonList(NpgsqlDataReader reader)
        {
            IList<User> list = new List<User>();
            while (reader.Read())
                list.Add(this.ToCommonSingle(reader));

            return list;
        }
    }
}
