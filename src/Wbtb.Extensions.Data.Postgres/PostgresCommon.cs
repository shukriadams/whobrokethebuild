using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    public class PostgresCommon
    {
        private static int connectionCount = 0;
        
        private static object threadLock = new object();

        public static int ConnectionCount() 
        {
            return connectionCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextPluginConfig"></param>
        /// <returns></returns>
        public static NpgsqlConnection GetConnection(PluginConfig contextPluginConfig)
        {
            string host = contextPluginConfig.Config.First(c => c.Key == "Host").Value.ToString();
            string password = contextPluginConfig.Config.First(c => c.Key == "Password").Value.ToString();
            string database = contextPluginConfig.Config.First(c => c.Key == "Database").Value.ToString();
            string user = contextPluginConfig.Config.First(c => c.Key == "User").Value.ToString();
            string connectionString = $"Host={host};Username={user};Password={password};Database={database};Include Error Detail=True;";


            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.StateChange += (object sender, System.Data.StateChangeEventArgs e) =>{
                lock(threadLock)
                    if (e.CurrentState == System.Data.ConnectionState.Closed)
                    {
                        connectionCount--;
                        if (connectionCount < 0)
                            connectionCount = 0;
                    }
            };

            lock (threadLock)
                connectionCount++;

            connection.Open();
            return connection;
        }


        public static int DestroyDatastore(PluginConfig contextPluginConfig) 
        {
            int updatedCount = 0;

            using (NpgsqlConnection connection = GetConnection(contextPluginConfig))
            {
                string createDbStructures = ResourceHelper.ReadResourceAsString(typeof(PostgresCommon), "sql.delete-structures.sql");
                using (NpgsqlCommand cmd = new NpgsqlCommand(createDbStructures, connection))
                    updatedCount = cmd.ExecuteNonQuery();
            }

            return updatedCount;
        }

        public static int InitializeDatastore(PluginConfig contextPluginConfig)
        {
            int updatedCount = 0;
            string query = @"SELECT EXISTS (
                SELECT FROM
                    information_schema.tables
                WHERE
                    table_name = 'sourceserver');";

            bool isInitialized = false;
            using (NpgsqlConnection connection = GetConnection(contextPluginConfig)) 
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    isInitialized = reader.GetBoolean(0);
                }

                if (isInitialized)
                    return updatedCount;

                string createDbStructures = ResourceHelper.ReadResourceAsString(typeof(PostgresCommon), "sql.create-structures.sql");
                using (NpgsqlCommand cmd = new NpgsqlCommand(createDbStructures, connection))
                    updatedCount = cmd.ExecuteNonQuery();
            }

            return updatedCount;
        }

        /// <summary>
        /// Executes an insert query on dataObject, returns a value assumed to be id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="dataObject"></param>
        /// <param name="parameterMapper"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static string InsertWithId<T>(PluginConfig contextPluginConfig, string query, T dataObject, ParameterMapper<T> parameterMapper, NpgsqlConnection connection)
        {
            bool close = false;

            try
            {
                if (connection == null)
                {
                    close = true;
                    connection = GetConnection(contextPluginConfig);
                }

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    parameterMapper(dataObject, cmd.Parameters);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        return reader.GetInt32(0).ToString();
                    }
                }
            }
            catch (Exception ex) 
            {
                SimpleDI di = new SimpleDI();
                Logger log = di.Resolve<Logger>();

                // 23505: should be postgres unique key constraint errors, log these instead of throwing exceptions, 
                // the key violations occur frequently when running with high thread concurrency, they should not occur
                // but the error is still there
                if (ex.Message.StartsWith("23505:"))
                {
                    log.Error(typeof(PostgresCommon), ex);
                    return "0";
                }
                else
                    throw;
            }
            finally
            {
                if (close)
                    connection.Dispose();
            }
        }

        /// <summary>
        /// Executes a query and returns count of rows affected instead of a data object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="contextPluginConfig"></param>
        /// <param name="query"></param>
        /// <param name="dataObject"></param>
        /// <param name="parameterMapper"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery<T>(PluginConfig contextPluginConfig, string query, T dataObject, ParameterMapper<T> parameterMapper, NpgsqlConnection connection)
        {
            bool close = false;

            try 
            {
                if (connection == null)
                {
                    close = true;
                    connection = GetConnection(contextPluginConfig);
                }

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    parameterMapper(dataObject, cmd.Parameters);
                    return cmd.ExecuteNonQuery();
                }

            }
            finally
            {
                if (close)
                    connection.Dispose();
            }
        }

        /// <summary>
        /// Executes a query and returns count of rows affected instead of a data object.
        /// </summary>
        /// <param name="contextPluginConfig"></param>
        /// <param name="query"></param>
        /// <param name="queryParameters"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(PluginConfig contextPluginConfig, string query, IEnumerable<QueryParameter> queryParameters, NpgsqlConnection connection)
        {
            bool close = false;

            try
            {
                if (connection == null)
                {
                    close = true;
                    connection = GetConnection(contextPluginConfig);
                }

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    foreach(QueryParameter parameter in queryParameters)
                        cmd.Parameters.AddWithValue(parameter.Name, parameter.Value);

                    return cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (close)
                    connection.Dispose();
            }
        }

        public static int Update<T>(PluginConfig contextPluginConfig, string query, T dataObject, ParameterMapper<T> parameterMapper, NpgsqlConnection connection)
        {
            bool close = false;

            try
            {
                if (connection == null)
                {
                    close = true;
                    connection = GetConnection(contextPluginConfig);
                }

                bool  isSignture = typeof(ISignature).IsAssignableFrom(typeof(T));
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    parameterMapper(dataObject, cmd.Parameters);
                    string new_signature = Guid.NewGuid().ToString();
                    if (isSignture)
                        cmd.Parameters.AddWithValue("new_signature", new_signature);

                    int result = cmd.ExecuteNonQuery();
                    if (isSignture && result == 0)
                        throw new WriteCollisionException(typeof(T).Name, ((ISignature)dataObject).Signature);

                    if (isSignture)
                        ((ISignature)dataObject).Signature = new_signature;

                    return result;
                }
            }
            finally
            {
                if (close)
                    connection.Dispose();
            }
        }

        public static int ClearAllTables(PluginConfig contextPluginConfig)
        {
            int deleted = 0;

            deleted += ClearTable(contextPluginConfig, "build");
            deleted += ClearTable(contextPluginConfig, "buildinvolvement");
            deleted += ClearTable(contextPluginConfig, "buildlogparseresult");
            deleted += ClearTable(contextPluginConfig, "buildserver");
            deleted += ClearTable(contextPluginConfig, "revision");
            deleted += ClearTable(contextPluginConfig, "job");
            deleted += ClearTable(contextPluginConfig, "session");
            deleted += ClearTable(contextPluginConfig, "sourceserver");
            deleted += ClearTable(contextPluginConfig, "usr");
            deleted += ClearTable(contextPluginConfig, "configurationstate");
            deleted += ClearTable(contextPluginConfig, "store");
            deleted += ClearTable(contextPluginConfig, "version");

            return deleted;
        }

        public static void ContactServer(PluginConfig contextPluginConfig)
        {
            using (NpgsqlConnection connection = GetConnection(contextPluginConfig))
            {
                string query = "SELECT * FROM information_schema.tables";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                    cmd.ExecuteNonQuery();
            };
        } 

        /// <summary>
        /// Do not make async, this is for testing only
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static int ClearTable(PluginConfig contextPluginConfig, string table)
        {
            using (NpgsqlConnection connection = GetConnection(contextPluginConfig))
            {
                string query = $"DELETE FROM \"{table}\";";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                    return cmd.ExecuteNonQuery();
            };
        }

        public static CommonT GetByQuery<CommonT>(NpgsqlConnection connection, PluginConfig contextPluginConfig, string query, IEnumerable<QueryParameter> parameters, IRecordConverter<CommonT> converter)
        {
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
            {
                if (parameters != null)
                    foreach(QueryParameter parameter in parameters)
                        cmd.Parameters.AddWithValue(parameter.Name, parameter.Value);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return converter.ToCommon(reader);
            }
        }

        public static CommonT GetByField<CommonT>(NpgsqlConnection connection, PluginConfig contextPluginConfig, string field, string value, string tableName, IRecordConverter<CommonT> converter)
        {
            string query = $"SELECT * FROM {tableName} WHERE {field}=@value";

            using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("value", value);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return converter.ToCommon(reader);
            }
        }

        public static CommonT GetById<CommonT>(NpgsqlConnection connection, PluginConfig contextPluginConfig, string id, string tableName, IRecordConverter<CommonT> converter)
        {
            string query = $"SELECT * FROM {tableName} WHERE id=@id";

            using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("id", int.Parse(id));

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    return converter.ToCommon(reader);
            }
        }

        public static bool Delete(NpgsqlConnection connection, PluginConfig contextPluginConfig, string table, string idName, string id)
        {
            string query = $"DELETE FROM \"{table}\" WHERE {idName}={id}";

            using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
            {
                int deleted = cmd.ExecuteNonQuery();
                return deleted > 0;
            }
        }

    }
}
