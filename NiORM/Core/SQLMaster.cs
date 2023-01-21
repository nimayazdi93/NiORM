using System.Data.SqlClient;

namespace NiORM.Core
{
    public class SqlMaster<T> where T : new()
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
                var fieldCount = reader.FieldCount;
                var record = new T();
                var schema = reader.GetColumnSchema();
                for (int i = 0; i < fieldCount; i++)
                {
                    ObjectDescriber<T, object>.SetValue(record, schema[i].ColumnName, reader.GetValue(i));
                }
                result.Add(record);
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
    }
}
