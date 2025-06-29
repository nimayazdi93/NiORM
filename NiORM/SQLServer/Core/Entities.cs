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

            try
            {
                this.ConnectionString = connectionString;
                SqlMaster = new SqlMaster<T>(connectionString);
                NiORMLogger.LogDebug($"Entities<{typeof(T).Name}> initialized successfully", "Entities.Constructor");
            }
            catch (Exception ex)
            {
                var error = $"Failed to initialize Entities<{typeof(T).Name}>: {ex.Message}";
                NiORMLogger.LogError(error, "Entities.Constructor", null, ex);
                throw new NiORMException(error, ex, null, "Constructor");
            }
        }

        private string TableName = ObjectDescriber<T, int>.GetTableName(new T());

        /// <summary>
        /// A method for first row in table
        /// </summary>
        /// <returns></returns>
        public T FirstOrDefault() => SqlMaster.Get(Query: $"SELECT TOP(1) * FROM {TableName}").FirstOrDefault();

        /// <summary>
        ///   A method for first row in table with conditions in TSQL
        /// </summary>
        /// <param name="Query">TSQL Query</param>
        /// <returns></returns>
        public T FirstOrDefault(string Query) => SqlMaster.Get($"SELECT TOP(1) * FROM {TableName} WHERE {Query}").FirstOrDefault();

        /// <summary>
        /// Finds an entity using its primary key (only for tables with a single primary key)
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>The entity if found, null otherwise</returns>
        /// <exception cref="ArgumentNullException">Thrown when id is null or empty</exception>
        /// <exception cref="NiORMValidationException">Thrown when the table doesn't have exactly one primary key</exception>
        /// <exception cref="NiORMException">Thrown when there's an error during the find operation</exception>
        public T Find(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                var error = "Primary key value cannot be null or empty";
                NiORMLogger.LogError(error, "Entities.Find");
                throw new ArgumentNullException(nameof(id), error);
            }

            try
            {
                NiORMLogger.LogDebug($"Finding {typeof(T).Name} with id: {id}", "Entities.Find");
                
                var Keys = ObjectDescriber<T, int>.GetPrimaryKeys(new T());

                if (Keys.Count != 1)
                {
                    var error = $"Table {typeof(T).Name} must have exactly one primary key for this Find method. Found {Keys.Count} primary keys.";
                    NiORMLogger.LogError(error, "Entities.Find");
                    throw new NiORMValidationException(error);
                }

                var Entity = new T();
                ObjectDescriber<T, string>.SetValue(Entity, Keys[0].Name, id);

                var query = $"SELECT TOP(1) * FROM {this.TableName} WHERE [{Keys[0].Name}] = {ObjectDescriber<T, string>.ToSqlFormat(Entity, Keys[0].Name)}";
                var result = SqlMaster.Get(query).FirstOrDefault();
                
                if (result != null)
                {
                    NiORMLogger.LogInfo($"Successfully found {typeof(T).Name} with id: {id}", "Entities.Find", query);
                }
                else
                {
                    NiORMLogger.LogWarning($"No {typeof(T).Name} found with id: {id}", "Entities.Find", query);
                }

                return result;
            }
            catch (NiORMException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var error = $"Unexpected error while finding {typeof(T).Name} with id {id}: {ex.Message}";
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
        public T Find(int id) => Find(id.ToString());

        /// <summary>
        /// A method for find an object using its primary key (Just for tables with two PK)
        /// </summary>
        /// <param name="firstId">Primary Key</param>
        /// <param name="secondId">Primary Key</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T Find(string firstId, string secondId)
        {
            var Keys = ObjectDescriber<T, int>.GetPrimaryKeyNames(new T());

            if (Keys.Count != 2)
                throw new Exception("The count of arguments are not same as PrimaryKeys");
            var Entity = new T();
            ObjectDescriber<T, int>.SetValue(Entity, Keys[0], int.Parse(firstId));
            ObjectDescriber<T, int>.SetValue(Entity, Keys[1], int.Parse(secondId));
            return SqlMaster.Get($@"SELECT TOP(1) * FROM {this.TableName}
                                    WHERE
                                        [{Keys[0]}]= {ObjectDescriber<T, string>.ToSqlFormat(Entity, Keys[0])}
                                        AND
                                         [{Keys[1]}]= {ObjectDescriber<T, string>.ToSqlFormat(Entity, Keys[1])}").FirstOrDefault();

        }

        /// <summary>
        /// A method for find an object using its primary key (Just for tables with two PK)
        /// </summary>
        /// <param name="firstId">Primary Key</param>
        /// <param name="secondId">Primary Key</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T Find(int firstId, string secondId) => Find(firstId.ToString(), secondId.ToString());

        public List<T> ToList()
        {
            var Properties = ObjectDescriber<T, string>.GetProperties(new T());
            return SqlMaster.Get($"SELECT * FROM {this.TableName}").ToList();
        }

        /// <summary>
        /// A method for fetching table with Query in where
        /// </summary>
        /// <param name="Query"></param>
        /// <returns></returns>
        public List<T> Query(string Query)
        {
            return SqlMaster.Get(Query).ToList();
        }

        /// <summary>
        /// A method for executing a TSQL command
        /// </summary>
        /// <param name="Query"></param>
        public void Execute(string Query)
        {
            SqlMaster.Execute(Query);
        }

        /// <summary>
        /// A method for fetching table with Query in where
        /// </summary>
        /// <param name="Query"></param>
        /// <returns></returns>
        public List<T> List(string Query)
        {
            return SqlMaster.Get($"SELECT * FROM {this.TableName} WHERE {Query}").ToList();
        }

        /// <summary>
        /// A extension for linq's WHERE
        /// </summary>
        /// <param name="Predict">A dictionary for predict</param>
        /// <returns></returns>
        public List<T> Where((string, string) Predict)
        {
            return List($" [{Predict.Item1}]='{Predict.Item2}'").ToList();

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

                case ExpressionType.MemberAccess:
                    var memberExpr = (MemberExpression)expression;
                    return memberExpr.Member.Name;

                case ExpressionType.Constant:
                    var constExpr = (ConstantExpression)expression;
                    var constExprString = ObjectDescriber<T, int>.ConvertToSqlFormat(constExpr.Value);
                    return constExprString;

                default:
                    throw new NotSupportedException($"Operation {expression.NodeType} is not supported.");
            }
        }

        private string GetType()
        {
            return new T().GetType().ToString().Split(".").LastOrDefault();
        }

        /// <summary>
        /// A method for adding a row and return it entirely
        /// </summary>
        /// <param name="entity">object we are adding</param>
        /// <exception cref="Exception"></exception>
        public T AddReturn(T entity)
        {

            var Type = GetType();
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

            var ListOfProperties = ObjectDescriber<T, int>
                                   .GetProperties(entity)
                                   .ToList();
            var PrimaryKeysDetails = ObjectDescriber<T, int>.GetPrimaryKeyDetails(entity).ToList();
            PrimaryKeysDetails.ForEach((pk) =>
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

            var Query = $@"INSERT INTO {this.TableName} 
                           (
                            {string.Join(",\n", ListOfProperties.Select(c => $"[{c}]").ToList())}
                            )
                            OUTPUT inserted.*
                            Values
                            (
                             {string.Join(",\n", ListOfProperties.Select(c => ObjectDescriber<T, int>.ToSqlFormat(entity, c)).ToList())}
                             )";

            var result = SqlMaster.Get(Query);
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
                NiORMLogger.LogDebug($"Adding new {typeof(T).Name} entity", "Entities.Add");

                var Type = GetType();
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

                var ListOfProperties = ObjectDescriber<T, int>
                                       .GetProperties(entity)
                                       .ToList();
                var PrimaryKeysDetails = ObjectDescriber<T, int>.GetPrimaryKeyDetails(entity).ToList();
                
                // Handle primary key properties
                PrimaryKeysDetails.ForEach((pk) =>
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

                var Query = $@"INSERT INTO {this.TableName} 
                               (
                                {string.Join(",\n", ListOfProperties.Select(c => $"[{c}]").ToList())}
                                )
                                VALUES
                                (
                                 {string.Join(",\n", ListOfProperties.Select(c => ObjectDescriber<T, int>.ToSqlFormat(entity, c)).ToList())}
                                 )";

                SqlMaster.Execute(Query);
                NiORMLogger.LogInfo($"Successfully added {typeof(T).Name} entity", "Entities.Add", Query);
            }
            catch (NiORMException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var error = $"Unexpected error while adding {typeof(T).Name} entity: {ex.Message}";
                NiORMLogger.LogError(error, "Entities.Add", null, ex);
                throw new NiORMException(error, ex, null, "Add");
            }
        }

        /// <summary>
        /// A method for editing a row
        /// </summary>
        /// <param name="entity">object we are editing</param>
        /// <exception cref="Exception"></exception>
        public void Edit(T entity)
        {
            var Type = GetType();

            if (entity is IView)
            {
                throw new Exception($"type: {Type} can't be added or edited because it's just View");
            }
            if (entity is IUpdatable updatable)
            {
                updatable.UpdatedDateTime = DateTime.Now;
                entity = (T)updatable;
            }
            var ListOfProperties = ObjectDescriber<T, int>
               .GetProperties(entity).ToList();

            var PrimaryKeys = ObjectDescriber<T, int>.GetPrimaryKeyNames(entity);
            var PrimaryKeysDetails = ObjectDescriber<T, int>.GetPrimaryKeyDetails(entity).ToList();

            ListOfProperties.AddRange(PrimaryKeysDetails.Where(c => c.IsAutoIncremental == false).Select(c => c.Name).ToList());
            ListOfProperties = ListOfProperties.Distinct().ToList();
            ListOfProperties = ListOfProperties.Where(c => !PrimaryKeysDetails.Any(cc => cc.Name == c && cc.IsAutoIncremental == true)).ToList();
            var Query = $@"UPDATE {this.TableName}
                           SET {string.Join(",\n", ListOfProperties.Select(c => $"[{c}]={ObjectDescriber<T, int>.ToSqlFormat(entity, c)}").ToList())}
                           WHERE {string.Join(" AND ", PrimaryKeys.Select(c => $" [{c}]= {ObjectDescriber<T, int>.ToSqlFormat(entity, c)}").ToList())}";

            SqlMaster.Execute(Query);


        }

        /// <summary>
        /// A method for removing a row
        /// </summary>
        /// <param name="entity">object we are removing</param>
        public void Remove(T entity)
        {
            var PrimaryKeys = ObjectDescriber<T, int>.GetPrimaryKeyNames(entity);

            var Query = $@"DELETE {this.TableName}  
                            WHERE {string.Join(" AND ", PrimaryKeys.Select(c => $" [{c}]= {ObjectDescriber<T, int>.ToSqlFormat(entity, c)}").ToList())}";
            SqlMaster.Execute(Query);
        }

        public List<T> List()
        {
            return this.ToList();
        }

        public List<T> ToList(string whereQuery)
        {
            return List(whereQuery);
        }
    }
}
