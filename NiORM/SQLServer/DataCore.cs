using NiORM.Mongo;
using NiORM.SQLServer.Core;
using NiORM.SQLServer.Interfaces;

namespace NiORM.SQLServer
{
    /// <summary>
    /// Main data access class for SQL Server operations using NiORM
    /// </summary>
    public class DataCore : IDataCore
    {
        private string ConnectionString { get; init; }

        /// <summary>
        /// Initializes a new instance of the DataCore class with the specified connection string
        /// </summary>
        /// <param name="connectionString">The SQL Server connection string</param>
        /// <exception cref="ArgumentNullException">Thrown when connection string is null or empty</exception>
        public DataCore(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                var error = "Connection string cannot be null or empty";
                NiORMLogger.LogError(error, "DataCore.Constructor");
                throw new ArgumentNullException(nameof(connectionString), error);
            }

            this.ConnectionString = connectionString;
            NiORMLogger.LogDebug("DataCore initialized successfully", "DataCore.Constructor");
        }

        /// <summary>
        /// Obsolete constructor - Do not use in production code
        /// </summary>
        [Obsolete("This constructor is obsolete and should not be used. Use DataCore(string connectionString) instead.")]
        public DataCore()
        {
            //Do Not Use!!!!
            NiORMLogger.LogWarning("Obsolete DataCore constructor used", "DataCore.Constructor");
        }

        /// <summary>
        /// Creates an entity collection for the specified table type
        /// </summary>
        /// <typeparam name="T">The entity type that implements ITable</typeparam>
        /// <returns>An IEntities collection for performing CRUD operations</returns>
        /// <exception cref="NiORMException">Thrown when there's an error creating the entity</exception>
        public IEntities<T> CreateEntity<T>() where T : ITable, new()
        {
            try
            {
                NiORMLogger.LogDebug($"Creating entity collection for {typeof(T).Name}", "DataCore.CreateEntity");
                var entity = new Entities<T>(ConnectionString);
                NiORMLogger.LogInfo($"Entity collection created successfully for {typeof(T).Name}", "DataCore.CreateEntity");
                return entity;
            }
            catch (Exception ex)
            {
                var error = $"Failed to create entity collection for {typeof(T).Name}: {ex.Message}";
                NiORMLogger.LogError(error, "DataCore.CreateEntity", null, ex);
                throw new NiORMException(error, ex, null, "CreateEntity");
            }
        }

        /// <summary>
        /// Executes a raw SQL query and returns the results mapped to the specified type
        /// </summary>
        /// <typeparam name="T">The type to map the results to</typeparam>
        /// <param name="query">The raw SQL query to execute</param>
        /// <returns>A list of results mapped to type T</returns>
        /// <exception cref="ArgumentNullException">Thrown when query is null or empty</exception>
        /// <exception cref="NiORMException">Thrown when there's an error executing the query</exception>
        public List<T> SqlRaw<T>(string query)  
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                var error = "Query cannot be null or empty";
                NiORMLogger.LogError(error, "DataCore.SqlRaw");
                throw new ArgumentNullException(nameof(query), error);
            }

            try
            {
                NiORMLogger.LogDebug($"Executing raw SQL query for {typeof(T).Name}", "DataCore.SqlRaw", query);
                SqlMaster<T> sqlMaster = new(ConnectionString);
                List<T> result = sqlMaster.Get(query);
                NiORMLogger.LogInfo($"Raw SQL query executed successfully, returned {result.Count} records", "DataCore.SqlRaw", query);
                return result;
            }
            catch (Exception ex)
            {
                var error = $"Failed to execute raw SQL query: {ex.Message}";
                NiORMLogger.LogError(error, "DataCore.SqlRaw", query, ex);
                throw;
            }
        }
    }
}
