using System.Linq.Expressions;

namespace NiORM.SQLServer.Interfaces
{
    /// <summary>
    /// Interface for entity collections providing CRUD operations and querying capabilities
    /// </summary>
    /// <typeparam name="T">The entity type that implements ITable</typeparam>
    public interface IEntities<T> where T : ITable
    {
        /// <summary>
        /// Retrieves all entities from the table
        /// </summary>
        /// <returns>A list of all entities</returns>
        List<T> ToList();

        /// <summary>
        /// Retrieves entities with a specific WHERE clause
        /// </summary>
        /// <param name="whereQuery">The WHERE clause query</param>
        /// <returns>A list of filtered entities</returns>
        List<T> ToList(string whereQuery);

        /// <summary>
        /// Adds a new entity and returns the added entity with generated values
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <returns>The added entity with any generated values</returns>
        T AddReturn(T entity);

        /// <summary>
        /// Adds a new entity to the database
        /// </summary>
        /// <param name="entity">The entity to add</param>
        void Add(T entity);

        /// <summary>
        /// Updates an existing entity in the database
        /// </summary>
        /// <param name="entity">The entity to update</param>
        void Edit(T entity);

        /// <summary>
        /// Removes an entity from the database
        /// </summary>
        /// <param name="entity">The entity to remove</param>
        void Remove(T entity);

        /// <summary>
        /// Filters entities using a simple property-value predicate
        /// </summary>
        /// <param name="Predict">A tuple containing property name and value</param>
        /// <returns>A list of filtered entities</returns>
        List<T> Where((string, string) Predict);

        /// <summary>
        /// Filters entities using a LINQ expression
        /// </summary>
        /// <param name="predicate">The LINQ expression predicate</param>
        /// <returns>A list of filtered entities</returns>
        List<T> Where(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Executes a custom SQL query and returns mapped entities
        /// </summary>
        /// <param name="Query">The custom SQL query</param>
        /// <returns>A list of entities from the query result</returns>
        List<T> Query(string Query);

        /// <summary>
        /// Finds an entity using composite primary keys (two keys)
        /// </summary>
        /// <param name="firstId">The first primary key value</param>
        /// <param name="secondId">The second primary key value</param>
        /// <returns>The entity if found, null otherwise</returns>
        T Find(int firstId, string secondId);

        /// <summary>
        /// Finds an entity using its integer primary key
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>The entity if found, null otherwise</returns>
        T Find(int id);

        /// <summary>
        /// Finds an entity using its string primary key
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>The entity if found, null otherwise</returns>
        T Find(string id);

        /// <summary>
        /// Returns the first entity matching the specified query, or null if none found
        /// </summary>
        /// <param name="Query">The WHERE clause query</param>
        /// <returns>The first matching entity or null</returns>
        T FirstOrDefault(string Query);

        /// <summary>
        /// Returns the first entity in the table, or null if the table is empty
        /// </summary>
        /// <returns>The first entity or null</returns>
        T FirstOrDefault();

        /// <summary>
        /// Executes a custom SQL command (non-query)
        /// </summary>
        /// <param name="Query">The SQL command to execute</param>
        void Execute(string Query);

        /// <summary>
        /// Retrieves all entities from the table
        /// </summary>
        /// <returns>A list of all entities</returns>
        List<T> List();

        /// <summary>
        /// Retrieves entities with a specific WHERE clause
        /// </summary>
        /// <param name="Query">The WHERE clause query</param>
        /// <returns>A list of filtered entities</returns>
        List<T> List(string Query);
    }
}