using Npgsql;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class BuildProcessorMapping
    {
        public static void MapParameters(Core.Common.BuildProcessor record, NpgsqlParameterCollection queryParameters)
        {
            if (!string.IsNullOrEmpty(record.Id))
                queryParameters.AddWithValue("id", int.Parse(record.Id));

            queryParameters.AddWithValue("signature", record.Signature);
            queryParameters.AddWithValue("buildid", int.Parse(record.BuildId));
            queryParameters.AddWithValue("processor", record.ProcessorKey);
            queryParameters.AddWithValue("status", (int)record.Status);
        }
    }
}
