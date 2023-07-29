using Npgsql;
using System;

namespace Wbtb.Extensions.Data.Postgres
{
    /// <summary>
    /// Abstracts away the overly verbose mess of Npgsql's connection + transaction system
    /// </summary>
    internal class PostgresConnectionWrapper : IDisposable
    {
        private readonly Postgres _parent;

        private NpgsqlConnection _ephemeralConnection;

        public PostgresConnectionWrapper(Postgres parent) 
        {
            _parent = parent;
        }

        public NpgsqlConnection Connection() 
        {
            if (_parent.Connection != null)
                return _parent.Connection;

            if (_ephemeralConnection != null)
                return _ephemeralConnection;

            _ephemeralConnection = PostgresCommon.GetConnection(_parent.ContextPluginConfig);
            return _ephemeralConnection;
            
        }

        public void Dispose() 
        {
            if (_ephemeralConnection != null) 
                _ephemeralConnection.Dispose();
        }
    }
}
