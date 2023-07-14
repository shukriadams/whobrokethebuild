using Npgsql;
using System;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class BuildInvolvementMapping 
    {
        public static void MapParameters(Core.Common.BuildInvolvement record, NpgsqlParameterCollection queryParameters)
        {
            if (!string.IsNullOrEmpty(record.Id))
                queryParameters.AddWithValue("id", int.Parse(record.Id));

            queryParameters.AddWithValue("signature", record.Signature);
            queryParameters.AddWithValue("buildid", int.Parse(record.BuildId));
            queryParameters.AddWithValue("blamescore", record.BlameScore);
            queryParameters.AddWithValue("comment", record.Comment == null ? (object)DBNull.Value : record.Comment);
            queryParameters.AddWithValue("isignoredfrombreakhistory", record.IsIgnoredFromBreakHistory);
            queryParameters.AddWithValue("inferredrevisionlink", record.InferredRevisionLink);
            queryParameters.AddWithValue("mappeduserid", record.MappedUserId == null ? (object)DBNull.Value : int.Parse(record.MappedUserId));
            queryParameters.AddWithValue("revisioncode", record.RevisionCode);
            queryParameters.AddWithValue("revisionid", record.RevisionId == null ? (object)DBNull.Value : int.Parse(record.RevisionId));
            queryParameters.AddWithValue("revisionlinkstatus", (int)record.RevisionLinkStatus);
            queryParameters.AddWithValue("userlinkstatus", (int)record.UserLinkStatus);
        }
    }
}
