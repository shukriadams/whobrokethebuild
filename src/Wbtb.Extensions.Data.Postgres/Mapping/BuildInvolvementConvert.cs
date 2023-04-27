﻿using System;
using System.Collections.Generic;
using Npgsql;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class BuildInvolvementConvert : IRecordConverter<BuildInvolvement>
    {
        private BuildInvolvement ToCommonSingle(NpgsqlDataReader reader)
        {
            return new BuildInvolvement
            {
                Id = reader["id"].ToString(),
                Signature = reader["signature"].ToString(),
                BuildId = reader["buildid"].ToString(),
                Blame = (Blame)reader["blame"],
                Comment = reader["comment"] == DBNull.Value ? null : reader["comment"].ToString(),
                IsIgnoredFromBreakHistory = bool.Parse(reader["isignoredfrombreakhistory"].ToString()),
                InferredRevisionLink = bool.Parse(reader["inferredrevisionlink"].ToString()),
                MappedUserId = reader["mappeduserid"] == DBNull.Value ? null : reader["mappeduserid"].ToString(),
                RevisionId = reader["revisionid"] == DBNull.Value ? null : reader["revisionid"].ToString(),
                RevisionCode = reader["revisioncode"].ToString(),
                RevisionLinkStatus = (LinkState)reader["revisionlinkstatus"],
                UserLinkStatus = (LinkState)reader["userlinkstatus"],
            };
        }

        public BuildInvolvement ToCommon(NpgsqlDataReader reader)
        {
            if (!reader.HasRows)
                return null;

            reader.Read();
            return ToCommonSingle(reader);
        }

        public IEnumerable<BuildInvolvement> ToCommonList(NpgsqlDataReader reader)
        {
            IList<BuildInvolvement> list = new List<BuildInvolvement>();
            while (reader.Read())
                list.Add(this.ToCommonSingle(reader));

            return list;
        }
    }
}
