using NiORM.SQLServer.Interfaces;
using System.Linq.Expressions;

namespace NiORM.SQLServer.Core
{
    /// <summary>
    /// An object for CRUD actions on database entities
    /// </summary>
    /// <typeparam name="T">Type of object related to table. It should be inherited from ITable</typeparam>
    public class Entities<T> : IEntities<T> where T : ITable, new()
    {
        private string? TableName = null;
        private List<string>? Keys = null;
        private List<string>? Properties = null;
        private List<PrimaryKeyDetails>? PrimaryKeysDetails = null;
        private string? TypeName = null;
        private string? RealTypeName = null;
        private string ConnectionString { get; init; }
        private SqlMaster<T> SqlMaster { get; init; }

        /// <summary>
        /// Initializes a new instance of the Entities class
        /// </summary>
        /// <param name="connectionString">The database connection string</param>
        /// <exception cref="ArgumentNullException">Thrown when connection string is null or empty</exception>
        /// <exception cref="NiORMException">Thrown when there's an error initializing the entity</exception>
        public Entities(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                var error = "Connection string cannot be null or empty";
                NiORMLogger.LogError(error, "Entities.Constructor");
                throw new ArgumentNullException(nameof(connectionString), error);
            }

            // Initialize TypeName early so it's available for logging
            TypeName = typeof(T).Name;

            try
            {
                this.ConnectionString = connectionString;
                SqlMaster = new SqlMaster<T>(connectionString);
                NiORMLogger.LogDebug($"Entities<{TypeName}> initialized successfully", "Entities.Constructor");
            }
            catch (Exception ex)
            {
                var error = $"Failed to initialize Entities<{TypeName}>: {ex.Message}";
                NiORMLogger.LogError(error, "Entities.Constructor", null, ex);
                throw new NiORMException(error, ex, null, "Constructor");
            }
            LoadRequirements();
        }

        private void LoadRequirements()
        {
            var obj = new T();
            TableName = ObjectDescriber<T, int>.GetTableName(obj);
            Keys = ObjectDescriber<T, int>.GetPrimaryKeyNames(obj);
            Properties = ObjectDescriber<T, int>.GetProperties(obj).ToList();
            PrimaryKeysDetails = ObjectDescriber<T, int>.GetPrimaryKeyDetails(obj).ToList();
            TypeName = typeof(T).Name;
            RealTypeName = obj.GetType().ToString().Split(".").LastOrDefault();
        }

        /// <summary>
        /// A method for first row in table
        /// </summary>
        /// <returns></returns>
        public T? FirstOrDefault()
        {
            return SqlMaster.Get(query: $"SELECT TOP(1) * FROM {TableName}").FirstOrDefault();
        }

        /// <summary>
        /// Returns the first entity matching the specified WHERE conditions
        /// WARNING: This method accepts raw SQL WHERE clauses. Use with caution and validate input.
        /// Consider using Where() method with specific parameters for better security.
        /// </summary>
        /// <param name="whereClause">The WHERE clause (without 'WHERE' keyword)</param>
        /// <returns>The first matching entity or null</returns>
        /// <exception cref="ArgumentException">Thrown when WHERE clause is null or empty</exception>
        /// <exception cref="NiORMException">Thrown when there's an error during execution</exception>
        public T? FirstOrDefault(string whereClause)
        {
            if (string.IsNullOrWhiteSpace(whereClause))
                throw new ArgumentException("WHERE clause cannot be null or empty", nameof(whereClause));

            try
            {
                // Log security warning for raw SQL usage
                NiORMLogger.LogWarning($"Using raw SQL WHERE clause in FirstOrDefault for {TypeName}: {whereClause}", "Entities.FirstOrDefault");

                var query = $"SELECT TOP(1) * FROM {TableName} WHERE {whereClause}";
                return SqlMaster.Get(query).FirstOrDefault();
            }
            catch (Exception ex)
            {
                var error = $"Error executing FirstOrDefault with WHERE clause for {TypeName}: {ex.Message}";
                NiORMLogger.LogError(error, "Entities.FirstOrDefault", null, ex);
                throw new NiORMException(error, ex, null, "FirstOrDefault");
            }
        }

        /// <summary>
        /// Finds an entity using its primary key (only for tables with a single primary key)
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>The entity if found, null otherwise</returns>
        /// <exception cref="ArgumentNullException">Thrown when id is null or empty</exception>
        /// <exception cref="NiORMValidationException">Thrown when the table doesn't have exactly one primary key</exception>
        /// <exception cref="NiORMException">Thrown when there's an error during the find operation</exception>
        public T? Find(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                var error = "Primary key value cannot be null or empty";
                NiORMLogger.LogError(error, "Entities.Find");
                throw new ArgumentNullException(nameof(id), error);
            }

            try
            {
                NiORMLogger.LogDebug($"Finding {TypeName} with id: {id}", "Entities.Find");


                if (Keys!.Count != 1)
                {
                    var error = $"Table {TypeName} must have exactly one primary key for this Find method. Found {Keys.Count} primary keys.";
                    NiORMLogger.LogError(error, "Entities.Find");
                    throw new NiORMValidationException(error);
                }

                // Use parameterized query to prevent SQL injection
                var paramHelper = new SqlParameterHelper();
                var paramName = paramHelper.AddParameter(id);

                var query = $"SELECT TOP(1) * FROM {this.TableName} WHERE [{Keys[0]}] = {paramName}";
                var result = SqlMaster.Get(query, paramHelper).FirstOrDefault();

                if (result != null)
                {
                    NiORMLogger.LogInfo($"Successfully found {TypeName} with id: {id}", "Entities.Find");
                }
                else
                {
                    NiORMLogger.LogWarning($"No {TypeName} found with id: {id}", "Entities.Find");
                }

                return result;
            }
            catch (NiORMException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var error = $"Unexpected error while finding {TypeName} with id {id}: {ex.Message}";
                NiORMLogger.LogError(error, "Entities.Find", null, ex);
                throw new NiORMException(error, ex, null, "Find");
            }
        }

        /// <summary>
        /// A method for find an object using its primary key (Just for tables with one PK)
        /// </summary>
        /// <param name="id">Primary Key</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T? Find(int id)
        {
            return Find(id.ToString());
        }

        /// <summary>
        /// Finds an entity using composite primary keys (exactly two keys)
        /// </summary>
        /// <param name="firstId">The first primary key value</param>
        /// <param name="secondId">The second primary key value</param>
        /// <returns>The entity if found, null otherwise</returns>
        /// <exception cref="ArgumentNullException">Thrown when any key value is null or empty</exception>
        /// <exception cref="NiORMValidationException">Thrown when the table doesn't have exactly two primary keys</exception>
        /// <exception cref="NiORMException">Thrown when there's an error during the find operation</exception>
        public T? Find(string firstId, string secondId)
        {
            if (string.IsNullOrWhiteSpace(firstId))
                throw new ArgumentNullException(nameof(firstId), "First primary key value cannot be null or empty");
            if (string.IsNullOrWhiteSpace(secondId))
                throw new ArgumentNullException(nameof(secondId), "Second primary key value cannot be null or empty");

            try
            {
                if (Keys?.Count != 2)
                {
                    var error = $"Table {TypeName} must have exactly two primary keys for this Find method. Found {Keys?.Count} primary keys.";
                    NiORMLogger.LogError(error, "Entities.Find");
                    throw new NiORMValidationException(error);
                }

                // Use parameterized query to prevent SQL injection
                var paramHelper = new SqlParameterHelper();
                var param1 = paramHelper.AddParameter(int.Parse(firstId));
                var param2 = paramHelper.AddParameter(int.Parse(secondId));

                var query = $@"SELECT TOP(1) * FROM {this.TableName}
                              WHERE [{Keys[0]}] = {param1}
                              AND [{Keys[1]}] = {param2}";

                var result = SqlMaster.Get(query, paramHelper).FirstOrDefault();

                if (result != null)
                {
                    NiORMLogger.LogInfo($"Successfully found {TypeName} with composite keys: {firstId}, {secondId}", "Entities.Find");
                }
                else
                {
                    NiORMLogger.LogWarning($"No {TypeName} found with composite keys: {firstId}, {secondId}", "Entities.Find");
                }

                return result;
            }
            catch (NiORMException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var error = $"Unexpected error while finding {TypeName} with composite keys {firstId}, {secondId}: {ex.Message}";
                NiORMLogger.LogError(error, "Entities.Find", null, ex);
                throw new NiORMException(error, ex, null, "Find");
            }
        }

        /// <summary>
        /// A method for find an object using its primary key (Just for tables with two PK)
        /// </summary>
        /// <param name="firstId">Primary Key</param>
        /// <param name="secondId">Primary Key</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T? Find(int firstId, string secondId)
        {
            return Find(firstId.ToString(), secondId.ToString());
        }

        public List<T> ToList()
        {
            return SqlMaster.Get($"SELECT * FROM {this.TableName}").ToList();
        }

        /// <summary>
        /// Executes a custom SQL query and returns mapped entities
        /// WARNING: This method accepts raw SQL queries. Use with extreme caution and validate input.
        /// Only use this for complex queries that cannot be handled by other methods.
        /// </summary>
        /// <param name="sqlQuery">The complete SQL query to execute</param>
        /// <returns>A list of entities from the query result</returns>
        /// <exception cref="ArgumentException">Thrown when SQL query is null or empty</exception>
        /// <exception cref="NiORMException">Thrown when there's an error during execution</exception>
        public List<T> Query(string sqlQuery)
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
                throw new ArgumentException("SQL query cannot be null or empty", nameof(sqlQuery));

            try
            {
                // Log security warning for raw SQL usage
                NiORMLogger.LogWarning($"Executing raw SQL query for {TypeName}: {sqlQuery}", "Entities.Query");

                return SqlMaster.Get(sqlQuery);
            }
            catch (Exception ex)
            {
                var error = $"Error executing custom SQL query for {TypeName}: {ex.Message}";
                NiORMLogger.LogError(error, "Entities.Query", sqlQuery, ex);
                throw new NiORMException(error, ex, sqlQuery, "Query");
            }
        }

        /// <summary>
        /// Executes a custom SQL command (non-query)
        /// WARNING: This method accepts raw SQL commands. Use with extreme caution and validate input.
        /// Only use this for operations that cannot be handled by Add, Edit, or Remove methods.
        /// </summary>
        /// <param name="sqlCommand">The SQL command to execute</param>
        /// <returns>Number of rows affected</returns>
        /// <exception cref="ArgumentException">Thrown when SQL command is null or empty</exception>
        /// <exception cref="NiORMException">Thrown when there's an error during execution</exception>
        public int Execute(string sqlCommand)
        {
            if (string.IsNullOrWhiteSpace(sqlCommand))
                throw new ArgumentException("SQL command cannot be null or empty", nameof(sqlCommand));

            try
            {
                // Log security warning for raw SQL usage
                NiORMLogger.LogWarning($"Executing raw SQL command for {TypeName}: {sqlCommand}", "Entities.Execute");

                return SqlMaster.Execute(sqlCommand);
            }
            catch (Exception ex)
            {
                var error = $"Error executing custom SQL command for {TypeName}: {ex.Message}";
                NiORMLogger.LogError(error, "Entities.Execute", sqlCommand, ex);
                throw new NiORMException(error, ex, sqlCommand, "Execute");
            }
        }

        /// <summary>
        /// Retrieves entities with a specified WHERE clause
        /// WARNING: This method accepts raw SQL WHERE clauses. Use with caution and validate input.
        /// Consider using Where() method with specific parameters for better security.
        /// </summary>
        /// <param name="whereClause">The WHERE clause (without 'WHERE' keyword)</param>
        /// <returns>A list of filtered entities</returns>
        /// <exception cref="ArgumentException">Thrown when WHERE clause is null or empty</exception>
        /// <exception cref="NiORMException">Thrown when there's an error during execution</exception>
        public List<T> List(string whereClause)
        {
            if (string.IsNullOrWhiteSpace(whereClause))
                throw new ArgumentException("WHERE clause cannot be null or empty", nameof(whereClause));

            try
            {
                // Log security warning for raw SQL usage
                NiORMLogger.LogWarning($"Using raw SQL WHERE clause in List for {TypeName}: {whereClause}", "Entities.List");

                var query = $"SELECT * FROM {this.TableName} WHERE {whereClause}";
                return SqlMaster.Get(query);
            }
            catch (Exception ex)
            {
                var error = $"Error executing List with WHERE clause for {TypeName}: {ex.Message}";
                NiORMLogger.LogError(error, "Entities.List", null, ex);
                throw new NiORMException(error, ex, null, "List");
            }
        }

        /// <summary>
        /// Filters entities using a simple property-value predicate (SQL injection safe)
        /// </summary>
        /// <param name="Predict">A tuple containing property name and value</param>
        /// <returns>A list of filtered entities</returns>
        /// <exception cref="ArgumentException">Thrown when property name is null or empty</exception>
        /// <exception cref="NiORMException">Thrown when there's an error during the filtering operation</exception>
        public List<T> Where((string, string) Predict)
        {
            if (string.IsNullOrWhiteSpace(Predict.Item1))
                throw new ArgumentException("Property name cannot be null or empty", nameof(Predict));

            try
            {
                // Use parameterized query to prevent SQL injection
                var paramHelper = new SqlParameterHelper();
                var paramName = paramHelper.AddParameter(Predict.Item2);

                var whereClause = $"[{Predict.Item1}] = {paramName}";
                var query = $"SELECT * FROM {this.TableName} WHERE {whereClause}";

                NiORMLogger.LogDebug($"Executing Where query for {TypeName}: {Predict.Item1} = {Predict.Item2}", "Entities.Where");
                return SqlMaster.Get(query, paramHelper);
            }
            catch (Exception ex)
            {
                var error = $"Error executing Where query for {TypeName}: {ex.Message}";
                NiORMLogger.LogError(error, "Entities.Where", null, ex);
                throw new NiORMException(error, ex, null, "Where");
            }
        }
        /// <summary>
        /// A extension for linq's WHERE
        /// </summary>
        /// <param name="Predict">A dictionary for predict</param>
        /// <returns></returns>
        public List<T> Where(Expression<Func<T, bool>> predicate)
        {
            var Query = ExpressionToString(predicate.Body);
            return List(Query);
        }

        private string ExpressionToString(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                    var binaryExpr = (BinaryExpression)expression;
                    return $"{ExpressionToString(binaryExpr.Left)} = {ExpressionToString(binaryExpr.Right)}";

                case ExpressionType.AndAlso:
                    binaryExpr = (BinaryExpression)expression;
                    return $"({ExpressionToString(binaryExpr.Left)} AND {ExpressionToString(binaryExpr.Right)})";

                case ExpressionType.OrElse:
                    binaryExpr = (BinaryExpression)expression;
                    return $"({ExpressionToString(binaryExpr.Left)} OR {ExpressionToString(binaryExpr.Right)})";

                case ExpressionType.NotEqual:
                    binaryExpr = (BinaryExpression)expression;
                    return $"{ExpressionToString(binaryExpr.Left)} != {ExpressionToString(binaryExpr.Right)}";

                case ExpressionType.MemberAccess:
                    return HandleMemberAccess((MemberExpression)expression);

                case ExpressionType.Constant:
                    var constExpr = (ConstantExpression)expression;
                    var constExprString = ObjectDescriber<T, int>.ConvertToSqlFormat(constExpr.Value);
                    return constExprString;

                default:
                    throw new NotSupportedException($"Operation {expression.NodeType} is not supported.");
            }
        }

        private string HandleMemberAccess(MemberExpression memberExpr)
        {
            // Handle .Date property on DateTime or DateTime?
            if (memberExpr.Member.Name == "Date")
            {
                var memberType = memberExpr.Member.DeclaringType;
                if (memberType == typeof(DateTime) || memberType == typeof(DateTime?))
                {
                    // Get the base expression (e.g., c.DateTime.Value or DateTime.Now)
                    // Skip .Value if present
                    var baseExpression = BuildMemberPath(memberExpr.Expression!, skipValue: true);
                    return $"CAST({baseExpression} AS DATE)";
                }
            }

            // Handle regular member accesses (including DateTime.Now and .Value)
            return BuildMemberPath(memberExpr, skipValue: false);
        }

        private string BuildMemberPath(Expression expression, bool skipValue = false)
        {
            if (expression == null)
            {
                return string.Empty;
            }

            switch (expression.NodeType)
            {
                case ExpressionType.Parameter:
                    // This is the parameter 'c' in the lambda - return empty to start building path
                    return string.Empty;

                case ExpressionType.MemberAccess:
                    var memberExpr = (MemberExpression)expression;
                    
                    // Handle DateTime.Now (static property, expression is null)
                    if (memberExpr.Member.DeclaringType == typeof(DateTime) && memberExpr.Member.Name == "Now" && memberExpr.Expression == null)
                    {
                        return "GETDATE()";
                    }

                    // Skip .Value property on nullable types if requested
                    if (skipValue && memberExpr.Member.Name == "Value")
                    {
                        return BuildMemberPath(memberExpr.Expression!, skipValue: true);
                    }

                    // Recursively build the path
                    var baseExpr = BuildMemberPath(memberExpr.Expression!, skipValue: skipValue);
                    var memberName = memberExpr.Member.Name;

                    // If base is empty, this is the first member (column name)
                    if (string.IsNullOrEmpty(baseExpr))
                    {
                        return $"[{memberName}]";
                    }
                    // If base is GETDATE(), we're accessing a property on DateTime.Now
                    else if (baseExpr == "GETDATE()")
                    {
                        return baseExpr;
                    }
                    // Otherwise, this is a nested property access
                    // For database columns, we typically only have one level, but handle nested gracefully
                    else
                    {
                        // Remove brackets from base if it's a column name, then add next member
                        var cleanBase = baseExpr.TrimStart('[').TrimEnd(']');
                        return $"[{cleanBase}.{memberName}]";
                    }

                case ExpressionType.Constant:
                    var constExpr = (ConstantExpression)expression;
                    return ObjectDescriber<T, int>.ConvertToSqlFormat(constExpr.Value);

                default:
                    // For other expression types, recursively process them
                    return ExpressionToString(expression);
            }
        }


        /// <summary>
        /// A method for adding a row and return it entirely
        /// </summary>
        /// <param name="entity">object we are adding</param>
        /// <exception cref="Exception"></exception>
        public T? AddReturn(T entity)
        {

            var Type = RealTypeName;
            if (entity is IView)
            {
                throw new Exception($"type: {Type} can't be added or edited because it's just View");
            }
            if (entity is IUpdatable updatable)
            {
                updatable.CreatedDateTime = DateTime.Now;
                updatable.UpdatedDateTime = DateTime.Now;
                entity = (T)updatable;
            }

            var ListOfProperties = Properties!.ToList();
            PrimaryKeysDetails!.ForEach((pk) =>
            {
                if (pk.IsAutoIncremental)
                {
                    if (!pk.IsGUID)
                    {
                        ListOfProperties.Remove(pk.Name);
                    }
                    else
                    {
                        ObjectDescriber<T, string>.SetValue(entity, pk.Name, Guid.NewGuid().ToString());
                    }
                }
                else
                {
                    ListOfProperties.Add(pk.Name);
                }
            });

            ListOfProperties = ListOfProperties.Distinct().ToList();

            // Use parameterized query to prevent SQL injection
            var paramHelper = new SqlParameterHelper();
            var valuesClause = paramHelper.BuildInsertValuesClause(ListOfProperties, entity);

            var query = $@"INSERT INTO {this.TableName} 
                           ({string.Join(", ", ListOfProperties.Select(c => $"[{c}]"))})
                           OUTPUT inserted.*
                           {valuesClause}";

            var result = SqlMaster.Get(query, paramHelper);
            return result.FirstOrDefault();

        }

        /// <summary>
        /// Adds a new entity to the database
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <exception cref="ArgumentNullException">Thrown when entity is null</exception>
        /// <exception cref="NiORMValidationException">Thrown when trying to add a view entity</exception>
        /// <exception cref="NiORMException">Thrown when there's an error during the add operation</exception>
        public void Add(T entity)
        {
            if (entity == null)
            {
                var error = "Entity cannot be null";
                NiORMLogger.LogError(error, "Entities.Add");
                throw new ArgumentNullException(nameof(entity), error);
            }

            try
            {
                NiORMLogger.LogDebug($"Adding new {TypeName} entity", "Entities.Add");

                var Type = RealTypeName;
                if (entity is IView)
                {
                    var error = $"Entity type {Type} cannot be added because it's a view";
                    NiORMLogger.LogError(error, "Entities.Add");
                    throw new NiORMValidationException(error, entity);
                }

                // Set timestamps for updatable entities
                if (entity is IUpdatable updatable)
                {
                    updatable.CreatedDateTime = DateTime.Now;
                    updatable.UpdatedDateTime = DateTime.Now;
                    entity = (T)updatable;
                    NiORMLogger.LogDebug("Set CreatedDateTime and UpdatedDateTime for updatable entity", "Entities.Add");
                }

                var ListOfProperties = Properties!.ToList();

                // Handle primary key properties
                PrimaryKeysDetails!.ForEach((pk) =>
                {
                    if (pk.IsAutoIncremental)
                    {
                        if (!pk.IsGUID)
                        {
                            ListOfProperties.Remove(pk.Name);
                            NiORMLogger.LogDebug($"Removed auto-incremental primary key {pk.Name} from insert properties", "Entities.Add");
                        }
                        else
                        {
                            ObjectDescriber<T, string>.SetValue(entity, pk.Name, Guid.NewGuid().ToString());
                            NiORMLogger.LogDebug($"Generated GUID for primary key {pk.Name}", "Entities.Add");
                        }
                    }
                    else
                    {
                        ListOfProperties.Add(pk.Name);
                    }
                });

                ListOfProperties = ListOfProperties.Distinct().ToList();

                // Use parameterized query to prevent SQL injection
                var paramHelper = new SqlParameterHelper();
                var valuesClause = paramHelper.BuildInsertValuesClause(ListOfProperties, entity);

                var query = $@"INSERT INTO {this.TableName} 
                               ({string.Join(", ", ListOfProperties.Select(c => $"[{c}]"))})
                               {valuesClause}";

                SqlMaster.Execute(query, paramHelper);
                NiORMLogger.LogInfo($"Successfully added {TypeName} entity", "Entities.Add");
            }
            catch (NiORMException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var error = $"Unexpected error while adding {TypeName} entity: {ex.Message}";
                NiORMLogger.LogError(error, "Entities.Add", null, ex);
                throw new NiORMException(error, ex, null, "Add");
            }
        }

        /// <summary>
        /// Updates an existing entity in the database (SQL injection safe)
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <exception cref="ArgumentNullException">Thrown when entity is null</exception>
        /// <exception cref="NiORMValidationException">Thrown when trying to update a view entity</exception>
        /// <exception cref="NiORMException">Thrown when there's an error during the update operation</exception>
        public void Edit(T entity)
        {
            if (entity == null)
            {
                var error = "Entity cannot be null";
                NiORMLogger.LogError(error, "Entities.Edit");
                throw new ArgumentNullException(nameof(entity), error);
            }

            try
            {
                var Type = RealTypeName;

                if (entity is IView)
                {
                    var error = $"Entity type {Type} cannot be updated because it's a view";
                    NiORMLogger.LogError(error, "Entities.Edit");
                    throw new NiORMValidationException(error, entity);
                }

                // Set timestamp for updatable entities
                if (entity is IUpdatable updatable)
                {
                    updatable.UpdatedDateTime = DateTime.Now;
                    entity = (T)updatable;
                    NiORMLogger.LogDebug("Set UpdatedDateTime for updatable entity", "Entities.Edit");
                }

                var ListOfProperties = Properties!.ToList();

                var PrimaryKeys = Keys;

                ListOfProperties.AddRange(PrimaryKeysDetails!.Where(c => c.IsAutoIncremental == false).Select(c => c.Name).ToList());
                ListOfProperties = ListOfProperties.Distinct().ToList();
                ListOfProperties = ListOfProperties.Where(c => !PrimaryKeysDetails!.Any(cc => cc.Name == c && cc.IsAutoIncremental == true)).ToList();

                // Use parameterized query to prevent SQL injection
                var paramHelper = new SqlParameterHelper();
                var setClause = paramHelper.BuildUpdateSetClause(ListOfProperties, entity);
                var whereClause = paramHelper.BuildPrimaryKeyWhereClause(PrimaryKeys!, entity);

                var query = $"UPDATE {this.TableName} {setClause} {whereClause}";

                var rowsAffected = SqlMaster.Execute(query, paramHelper);
                NiORMLogger.LogInfo($"Successfully updated {TypeName} entity, {rowsAffected} rows affected", "Entities.Edit");
            }
            catch (NiORMException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var error = $"Unexpected error while updating {TypeName} entity: {ex.Message}";
                NiORMLogger.LogError(error, "Entities.Edit", null, ex);
                throw new NiORMException(error, ex, null, "Edit");
            }
        }

        /// <summary>
        /// Removes an entity from the database (SQL injection safe)
        /// </summary>
        /// <param name="entity">The entity to remove</param>
        /// <exception cref="ArgumentNullException">Thrown when entity is null</exception>
        /// <exception cref="NiORMException">Thrown when there's an error during the remove operation</exception>
        public void Remove(T entity)
        {
            if (entity == null)
            {
                var error = "Entity cannot be null";
                NiORMLogger.LogError(error, "Entities.Remove");
                throw new ArgumentNullException(nameof(entity), error);
            }

            try
            {

                if (Keys == null || !Keys.Any())
                {
                    var error = $"Entity {TypeName} must have at least one primary key to be removed";
                    NiORMLogger.LogError(error, "Entities.Remove");
                    throw new NiORMValidationException(error, entity);
                }

                // Use parameterized query to prevent SQL injection
                var paramHelper = new SqlParameterHelper();
                var whereClause = paramHelper.BuildPrimaryKeyWhereClause(Keys, entity);

                var query = $"DELETE FROM {this.TableName} {whereClause}";

                var rowsAffected = SqlMaster.Execute(query, paramHelper);
                NiORMLogger.LogInfo($"Successfully removed {TypeName} entity, {rowsAffected} rows affected", "Entities.Remove");
            }
            catch (NiORMException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var error = $"Unexpected error while removing {TypeName} entity: {ex.Message}";
                NiORMLogger.LogError(error, "Entities.Remove", null, ex);
                throw new NiORMException(error, ex, null, "Remove");
            }
        }

        public List<T> List()
        {
            return this.ToList();
        }

        public List<T> ToList(string whereQuery)
        {
            return List(whereQuery);
        }

        /// <summary>
        /// Safe method to filter entities using multiple property-value conditions (SQL injection safe)
        /// </summary>
        /// <param name="conditions">Dictionary of column names and their values</param>
        /// <returns>A list of filtered entities</returns>
        /// <exception cref="ArgumentException">Thrown when conditions is null or empty</exception>
        /// <exception cref="NiORMException">Thrown when there's an error during filtering</exception>
        public List<T> WhereMultiple(Dictionary<string, object?> conditions)
        {
            if (conditions == null || !conditions.Any())
                throw new ArgumentException("Conditions cannot be null or empty", nameof(conditions));

            try
            {
                // Use parameterized query to prevent SQL injection
                var paramHelper = new SqlParameterHelper();
                var whereClause = paramHelper.BuildWhereClause(conditions);

                var query = $"SELECT * FROM {this.TableName} {whereClause}";

                NiORMLogger.LogDebug($"Executing WhereMultiple query for {TypeName} with {conditions.Count} conditions", "Entities.WhereMultiple");
                return SqlMaster.Get(query, paramHelper);
            }
            catch (Exception ex)
            {
                var error = $"Error executing WhereMultiple query for {TypeName}: {ex.Message}";
                NiORMLogger.LogError(error, "Entities.WhereMultiple", null, ex);
                throw new NiORMException(error, ex, null, "WhereMultiple");
            }
        }

        /// <summary>
        /// Safe method to find entities by a single property value (SQL injection safe)
        /// </summary>
        /// <param name="propertyName">The property name to filter by</param>
        /// <param name="value">The value to search for</param>
        /// <returns>A list of entities matching the criteria</returns>
        /// <exception cref="ArgumentException">Thrown when property name is null or empty</exception>
        /// <exception cref="NiORMException">Thrown when there's an error during the search</exception>
        public List<T> FindByProperty(string propertyName, object? value)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("Property name cannot be null or empty", nameof(propertyName));

            try
            {
                var conditions = new Dictionary<string, object?> { { propertyName, value } };
                return WhereMultiple(conditions);
            }
            catch (Exception ex)
            {
                var error = $"Error finding {TypeName} by property {propertyName}: {ex.Message}";
                NiORMLogger.LogError(error, "Entities.FindByProperty", null, ex);
                throw new NiORMException(error, ex, null, "FindByProperty");
            }
        }

        /// <summary>
        /// Safe method to get the first entity matching multiple conditions (SQL injection safe)
        /// </summary>
        /// <param name="conditions">Dictionary of column names and their values</param>
        /// <returns>The first matching entity or null</returns>
        /// <exception cref="ArgumentException">Thrown when conditions is null or empty</exception>
        /// <exception cref="NiORMException">Thrown when there's an error during the search</exception>
        public T? FirstOrDefaultMultiple(Dictionary<string, object?> conditions)
        {
            if (conditions == null || !conditions.Any())
                throw new ArgumentException("Conditions cannot be null or empty", nameof(conditions));

            try
            {
                // Use parameterized query to prevent SQL injection
                var paramHelper = new SqlParameterHelper();
                var whereClause = paramHelper.BuildWhereClause(conditions);

                var query = $"SELECT TOP(1) * FROM {this.TableName} {whereClause}";

                NiORMLogger.LogDebug($"Executing FirstOrDefaultMultiple query for {TypeName} with {conditions.Count} conditions", "Entities.FirstOrDefaultMultiple");
                return SqlMaster.Get(query, paramHelper).FirstOrDefault();
            }
            catch (Exception ex)
            {
                var error = $"Error executing FirstOrDefaultMultiple query for {TypeName}: {ex.Message}";
                NiORMLogger.LogError(error, "Entities.FirstOrDefaultMultiple", null, ex);
                throw new NiORMException(error, ex, null, "FirstOrDefaultMultiple");
            }
        }

    }
}
