using System.Data.SqlClient;

namespace NiORM.SQLServer.Core
{
    public class SqlMaster<T> 
    {
        private string Connection { get; init; }
        public SqlMaster(string ConnectionString)
        {
            Connection = ConnectionString;
        }

        public List<T> Get(string Query)
        {
            using SqlConnection sqlConnection = new SqlConnection(Connection);
            sqlConnection.Open();
            var command = new SqlCommand(Query, sqlConnection);
            using SqlDataReader reader = command.ExecuteReader();
            var result = new List<T>();
            while (reader.Read())
            {
                if (IsBasicType(typeof(T)))
                {
                    result.Add((T)Convert.ChangeType(reader.GetValue(0), typeof(T)));
                }
                else
                {
                    var fieldCount = reader.FieldCount;
                    var record = Activator.CreateInstance<T>();

                    var schema = reader.GetColumnSchema();
                    for (int i = 0; i < fieldCount; i++)
                    {
                        ObjectDescriber<T, object>.SetValue(record, schema[i].ColumnName, reader.GetValue(i));
                    }
                    result.Add(record);
                }
            }
            reader.Close();
            sqlConnection.Close();
            return result;
        }

        public void Execute(string Query)
        {
            using SqlConnection sqlConnection = new SqlConnection(Connection);
            sqlConnection.Open();
            var command = new SqlCommand(Query, sqlConnection);
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read()) { }
            reader.Close();
            sqlConnection.Close();
            return;
        }

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
