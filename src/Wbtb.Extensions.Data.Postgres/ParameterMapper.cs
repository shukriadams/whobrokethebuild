using Npgsql;

namespace Wbtb.Extensions.Data.Postgres
{
    public delegate void ParameterMapper<T>(T dataObject, NpgsqlParameterCollection parameterCollection);
}
