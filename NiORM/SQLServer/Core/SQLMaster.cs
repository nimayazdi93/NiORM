using System.Data.SqlClient;

namespace NiORM.SQLServer.Core
{
    /// <summary>
    /// Core SQL execution class for database operations
    /// </summary>
    /// <typeparam name="T">The type of entity being operated on</typeparam>
    public class SqlMaster<T> 
    {
        private string Connection { get; init; }

        /// <summary>
        /// Initializes a new instance of the SqlMaster class
        /// </summary>
        /// <param name="connectionString">The database connection string</param>
        /// <exception cref="ArgumentNullException">Thrown when connection string is null or empty</exception>
        public SqlMaster(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                var error = "Connection string cannot be null or empty";
                NiORMLogger.LogError(error, "SqlMaster.Constructor");
                throw new ArgumentNullException(nameof(connectionString), error);
            }

            Connection = connectionString;
            NiORMLogger.LogDebug($"SqlMaster<{typeof(T).Name}> initialized", "SqlMaster.Constructor");
        }

        /// <summary>
        /// Executes a SELECT query and returns the results as a list of entities
        /// </summary>
        /// <param name="query">The SQL query to execute</param>
        /// <param name="parameterHelper">Optional parameter helper for parameterized queries</param>
        /// <returns>A list of entities of type T</returns>
        /// <exception cref="NiORMException">Thrown when there's an error executing the query</exception>
        /// <exception cref="NiORMConnectionException">Thrown when there's a connection error</exception>
        /// <exception cref="NiORMMappingException">Thrown when there's an error mapping results to entities</exception>
        public List<T> Get(string query, SqlParameterHelper? parameterHelper = null)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                var error = "Query cannot be null or empty";
                NiORMLogger.LogError(error, "SqlMaster.Get");
                throw new NiORMException(error, query, "Get");
            }

            var debugQuery = parameterHelper?.GetDebugQuery(query) ?? query;
            NiORMLogger.LogDebug($"Executing GET query for {typeof(T).Name}", "SqlMaster.Get", debugQuery);

            try
            {
                using SqlConnection sqlConnection = new SqlConnection(Connection);
                
                try
                {
                    sqlConnection.Open();
                    NiORMLogger.LogDebug("Database connection opened successfully", "SqlMaster.Get");
                }
                catch (Exception ex)
                {
                    var error = $"Failed to open database connection: {ex.Message}";
                    NiORMLogger.LogError(error, "SqlMaster.Get", debugQuery, ex);
                    throw new NiORMConnectionException(error, ex);
                }

                var command = new SqlCommand(query, sqlConnection);
                
                // Apply parameters if provided
                parameterHelper?.ApplyParameters(command);

                using SqlDataReader reader = command.ExecuteReader();
                var result = new List<T>();
                
                try
                {
                    while (reader.Read())
                    {
                        if (IsBasicType(typeof(T)))
                        {
                            var value = reader.GetValue(0);
                            if (value == DBNull.Value)
                            {
                                result.Add(default(T)!);
                            }
                            else
                            {
                                result.Add((T)Convert.ChangeType(value, typeof(T)));
                            }
                        }
                        else
                        {
                            var fieldCount = reader.FieldCount;
                            var record = Activator.CreateInstance<T>();

                            var schema = reader.GetColumnSchema();
                            for (int i = 0; i < fieldCount; i++)
                            {
                                try
                                {
                                    var value = reader.GetValue(i);
                                    if (value != DBNull.Value)
                                    {
                                        ObjectDescriber<T, object>.SetValue(record, schema[i].ColumnName, value);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    NiORMLogger.LogWarning($"Failed to set property {schema[i].ColumnName}: {ex.Message}", "SqlMaster.Get", debugQuery);
                                }
                            }
                            result.Add(record);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var error = $"Error while reading query results: {ex.Message}";
                    NiORMLogger.LogError(error, "SqlMaster.Get", debugQuery, ex);
                    throw new NiORMMappingException(error, ex);
                }

                reader.Close();
                sqlConnection.Close();
                
                NiORMLogger.LogInfo($"Query executed successfully, returned {result.Count} records", "SqlMaster.Get", debugQuery);
                return result;
            }
            catch (NiORMException)
            {
                // Re-throw our custom exceptions
                throw;
            }
            catch (Exception ex)
            {
                var error = $"Unexpected error during query execution: {ex.Message}";
                NiORMLogger.LogError(error, "SqlMaster.Get", debugQuery, ex);
                throw new NiORMException(error, ex, debugQuery, "Get");
            }
        }

        /// <summary>
        /// Executes a non-query SQL command (INSERT, UPDATE, DELETE, etc.)
        /// </summary>
        /// <param name="query">The SQL command to execute</param>
        /// <param name="parameterHelper">Optional parameter helper for parameterized queries</param>
        /// <returns>Number of rows affected</returns>
        /// <exception cref="NiORMException">Thrown when there's an error executing the command</exception>
        /// <exception cref="NiORMConnectionException">Thrown when there's a connection error</exception>
        public int Execute(string query, SqlParameterHelper? parameterHelper = null)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                var error = "Query cannot be null or empty";
                NiORMLogger.LogError(error, "SqlMaster.Execute");
                throw new NiORMException(error, query, "Execute");
            }

            var debugQuery = parameterHelper?.GetDebugQuery(query) ?? query;
            NiORMLogger.LogDebug($"Executing command for {typeof(T).Name}", "SqlMaster.Execute", debugQuery);

            try
            {
                using SqlConnection sqlConnection = new SqlConnection(Connection);
                
                try
                {
                    sqlConnection.Open();
                    NiORMLogger.LogDebug("Database connection opened successfully", "SqlMaster.Execute");
                }
                catch (Exception ex)
                {
                    var error = $"Failed to open database connection: {ex.Message}";
                    NiORMLogger.LogError(error, "SqlMaster.Execute", debugQuery, ex);
                    throw new NiORMConnectionException(error, ex);
                }

                var command = new SqlCommand(query, sqlConnection);
                
                // Apply parameters if provided
                parameterHelper?.ApplyParameters(command);

                var rowsAffected = command.ExecuteNonQuery();
                
                sqlConnection.Close();
                NiORMLogger.LogInfo($"Command executed successfully, {rowsAffected} rows affected", "SqlMaster.Execute", debugQuery);
                return rowsAffected;
            }
            catch (NiORMException)
            {
                // Re-throw our custom exceptions
                throw;
            }
            catch (Exception ex)
            {
                var error = $"Unexpected error during command execution: {ex.Message}";
                NiORMLogger.LogError(error, "SqlMaster.Execute", debugQuery, ex);
                throw new NiORMException(error, ex, debugQuery, "Execute");
            }
        }

        /// <summary>
        /// Determines whether the specified type is a basic .NET type that can be directly converted
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>True if the type is a basic type, false otherwise</returns>
        private bool IsBasicType(Type type)
        {
            var basicTypes = new List<Type>
            {
                typeof(int), typeof(string), typeof(bool),
                typeof(double), typeof(float), typeof(decimal),
                typeof(DateTime), typeof(Guid), typeof(byte),
                typeof(short), typeof(long), typeof(char)
            };

            return basicTypes.Contains(type) || type.IsEnum;
        }
    }
}
