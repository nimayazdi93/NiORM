using System.Data.SqlClient;
using System.Data;
using System.Text;

namespace NiORM.SQLServer.Core
{
    /// <summary>
    /// Helper class for building safe SQL queries with parameters to prevent SQL injection
    /// </summary>
    public class SqlParameterHelper
    {
        private readonly List<SqlParameter> _parameters;
        private int _parameterCounter;

        /// <summary>
        /// Initializes a new instance of the SqlParameterHelper
        /// </summary>
        public SqlParameterHelper()
        {
            _parameters = new List<SqlParameter>();
            _parameterCounter = 0;
        }

        /// <summary>
        /// Gets the list of SQL parameters
        /// </summary>
        public IReadOnlyList<SqlParameter> Parameters => _parameters.AsReadOnly();

        /// <summary>
        /// Adds a parameter and returns the parameter name to use in the query
        /// </summary>
        /// <param name="value">The parameter value</param>
        /// <param name="dbType">Optional database type</param>
        /// <returns>The parameter name (e.g., @param1)</returns>
        public string AddParameter(object? value, SqlDbType? dbType = null)
        {
            _parameterCounter++;
            var paramName = $"@param{_parameterCounter}";
            
            var parameter = new SqlParameter(paramName, value ?? DBNull.Value);
            
            if (dbType.HasValue)
            {
                parameter.SqlDbType = dbType.Value;
            }
            else
            {
                // Auto-detect type based on value
                SetParameterType(parameter, value);
            }

            _parameters.Add(parameter);
            return paramName;
        }

        /// <summary>
        /// Adds a named parameter and returns the parameter name
        /// </summary>
        /// <param name="name">Parameter name (without @)</param>
        /// <param name="value">The parameter value</param>
        /// <param name="dbType">Optional database type</param>
        /// <returns>The parameter name with @ prefix</returns>
        public string AddNamedParameter(string name, object? value, SqlDbType? dbType = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Parameter name cannot be null or empty", nameof(name));

            var paramName = name.StartsWith("@") ? name : $"@{name}";
            
            // Check if parameter already exists
            if (_parameters.Any(p => p.ParameterName.Equals(paramName, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException($"Parameter {paramName} already exists", nameof(name));

            var parameter = new SqlParameter(paramName, value ?? DBNull.Value);
            
            if (dbType.HasValue)
            {
                parameter.SqlDbType = dbType.Value;
            }
            else
            {
                SetParameterType(parameter, value);
            }

            _parameters.Add(parameter);
            return paramName;
        }

        /// <summary>
        /// Creates WHERE clause with parameters
        /// </summary>
        /// <param name="conditions">Dictionary of column name and value pairs</param>
        /// <returns>WHERE clause string with parameter placeholders</returns>
        public string BuildWhereClause(Dictionary<string, object?> conditions)
        {
            if (conditions == null || !conditions.Any())
                return string.Empty;

            var whereParts = new List<string>();
            
            foreach (var condition in conditions)
            {
                var paramName = AddParameter(condition.Value);
                whereParts.Add($"[{condition.Key}] = {paramName}");
            }

            return $"WHERE {string.Join(" AND ", whereParts)}";
        }

        /// <summary>
        /// Creates WHERE clause for primary keys
        /// </summary>
        /// <param name="primaryKeys">List of primary key names</param>
        /// <param name="entity">Entity with primary key values</param>
        /// <returns>WHERE clause string with parameter placeholders</returns>
        public string BuildPrimaryKeyWhereClause<T>(List<string> primaryKeys, T entity)
        {
            if (primaryKeys == null || !primaryKeys.Any())
                throw new ArgumentException("Primary keys cannot be null or empty", nameof(primaryKeys));

            var whereParts = new List<string>();
            
            foreach (var pkName in primaryKeys)
            {
                var value = ObjectDescriber<T, object>.GetValue(entity, pkName);
                var paramName = AddParameter(value);
                whereParts.Add($"[{pkName}] = {paramName}");
            }

            return $"WHERE {string.Join(" AND ", whereParts)}";
        }

        /// <summary>
        /// Creates INSERT values clause with parameters
        /// </summary>
        /// <param name="properties">List of property names</param>
        /// <param name="entity">Entity with values</param>
        /// <returns>VALUES clause string with parameter placeholders</returns>
        public string BuildInsertValuesClause<T>(List<string> properties, T entity)
        {
            if (properties == null || !properties.Any())
                throw new ArgumentException("Properties cannot be null or empty", nameof(properties));

            var valueParams = new List<string>();
            
            foreach (var property in properties)
            {
                var value = GetEntityPropertyValue(entity, property);
                var paramName = AddParameter(value);
                valueParams.Add(paramName);
            }

            return $"VALUES ({string.Join(", ", valueParams)})";
        }

        /// <summary>
        /// Creates UPDATE SET clause with parameters
        /// </summary>
        /// <param name="properties">List of property names to update</param>
        /// <param name="entity">Entity with new values</param>
        /// <returns>SET clause string with parameter placeholders</returns>
        public string BuildUpdateSetClause<T>(List<string> properties, T entity)
        {
            if (properties == null || !properties.Any())
                throw new ArgumentException("Properties cannot be null or empty", nameof(properties));

            var setParts = new List<string>();
            
            foreach (var property in properties)
            {
                var value = GetEntityPropertyValue(entity, property);
                var paramName = AddParameter(value);
                setParts.Add($"[{property}] = {paramName}");
            }

            return $"SET {string.Join(", ", setParts)}";
        }

        /// <summary>
        /// Applies parameters to a SQL command
        /// </summary>
        /// <param name="command">The SQL command</param>
        public void ApplyParameters(SqlCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            command.Parameters.Clear();
            foreach (var parameter in _parameters)
            {
                command.Parameters.Add(parameter);
            }
        }

        /// <summary>
        /// Clears all parameters
        /// </summary>
        public void Clear()
        {
            _parameters.Clear();
            _parameterCounter = 0;
        }

        /// <summary>
        /// Gets the debug string showing the query with parameter values (for logging only)
        /// </summary>
        /// <param name="query">The parameterized query</param>
        /// <returns>Query with parameter values substituted (for debugging)</returns>
        public string GetDebugQuery(string query)
        {
            var debugQuery = query;
            foreach (var param in _parameters)
            {
                var value = param.Value == DBNull.Value ? "NULL" : 
                           param.Value is string ? $"'{param.Value}'" : 
                           param.Value?.ToString() ?? "NULL";
                debugQuery = debugQuery.Replace(param.ParameterName, value);
            }
            return debugQuery;
        }

        /// <summary>
        /// Sets the appropriate SqlDbType for a parameter based on its value
        /// </summary>
        /// <param name="parameter">The parameter to configure</param>
        /// <param name="value">The value to analyze</param>
        private void SetParameterType(SqlParameter parameter, object? value)
        {
            if (value == null)
            {
                parameter.SqlDbType = SqlDbType.Variant;
                return;
            }

            switch (value)
            {
                case string:
                    parameter.SqlDbType = SqlDbType.NVarChar;
                    parameter.Size = ((string)value).Length > 4000 ? -1 : 4000;
                    break;
                case int:
                    parameter.SqlDbType = SqlDbType.Int;
                    break;
                case long:
                    parameter.SqlDbType = SqlDbType.BigInt;
                    break;
                case short:
                    parameter.SqlDbType = SqlDbType.SmallInt;
                    break;
                case byte:
                    parameter.SqlDbType = SqlDbType.TinyInt;
                    break;
                case bool:
                    parameter.SqlDbType = SqlDbType.Bit;
                    break;
                case DateTime:
                    parameter.SqlDbType = SqlDbType.DateTime2;
                    break;
                case decimal:
                    parameter.SqlDbType = SqlDbType.Decimal;
                    break;
                case double:
                    parameter.SqlDbType = SqlDbType.Float;
                    break;
                case float:
                    parameter.SqlDbType = SqlDbType.Real;
                    break;
                case Guid:
                    parameter.SqlDbType = SqlDbType.UniqueIdentifier;
                    break;
                case char:
                    parameter.SqlDbType = SqlDbType.NChar;
                    parameter.Size = 1;
                    break;
                default:
                    if (value.GetType().IsEnum)
                    {
                        parameter.SqlDbType = SqlDbType.Int;
                        parameter.Value = (int)value;
                    }
                    else
                    {
                        parameter.SqlDbType = SqlDbType.Variant;
                    }
                    break;
            }
        }

        /// <summary>
        /// Gets the value of a property from an entity safely
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entity">The entity</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>Property value</returns>
        private object? GetEntityPropertyValue<T>(T entity, string propertyName)
        {
            try
            {
                return ObjectDescriber<T, object>.GetValue(entity, propertyName);
            }
            catch
            {
                // If we can't get the value, try reflection directly
                var property = entity.GetType().GetProperty(propertyName);
                return property?.GetValue(entity);
            }
        }
    }
} 