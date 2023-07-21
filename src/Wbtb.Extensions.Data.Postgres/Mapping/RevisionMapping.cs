using Npgsql;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class RevisionMapping 
    {
        public static void MapParameters(Core.Common.Revision revision, NpgsqlParameterCollection queryParameters)
        {
            if (!string.IsNullOrEmpty(revision.Id))
                queryParameters.AddWithValue("id", int.Parse(revision.Id));

 
            queryParameters.AddWithValue("signature", revision.Signature);
            queryParameters.AddWithValue("code", revision.Code);
            queryParameters.AddWithValue("sourceserverid", int.Parse(revision.SourceServerId));
            queryParameters.AddWithValue("created", revision.Created);
            queryParameters.AddWithValue("usr", revision.User);
            queryParameters.AddWithValue("files", JsonConvert.SerializeObject(revision.Files));
            queryParameters.AddWithValue("description", revision.Description);
        }
    }
}



