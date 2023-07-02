using Npgsql;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class RevisionMapping 
    {
        public static void MapParameters(Core.Common.Revision revision, NpgsqlParameterCollection collection)
        {
            if (!string.IsNullOrEmpty(revision.Id))
                collection.AddWithValue("id", int.Parse(revision.Id));

 
            collection.AddWithValue("signature", revision.Signature);
            collection.AddWithValue("code", revision.Code);
            collection.AddWithValue("sourceserverid", int.Parse(revision.SourceServerId));
            collection.AddWithValue("created", revision.Created);
            collection.AddWithValue("usr", revision.User);
            collection.AddWithValue("files", JsonConvert.SerializeObject(revision.Files));
            collection.AddWithValue("description", revision.Description);
        }
    }
}



