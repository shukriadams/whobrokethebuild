using Npgsql;

namespace Wbtb.Extensions.Data.Postgres
{
    public interface IRecordConverter<Common>
    {
        Common ToCommon(NpgsqlDataReader reader);
    }
}
