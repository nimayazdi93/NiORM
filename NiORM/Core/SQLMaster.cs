using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiORM.Core
{
    public  class SQLMaster<T> where T : new()
    {
        private  string Connection { get; set; }
        public SQLMaster(string ConnectionString)
        {
            Connection = ConnectionString;
        }
        public  List<T> Get(string Query)
        {
            using SqlConnection sqlConnection = new SqlConnection(Connection);
            sqlConnection.Open();
            var command = new SqlCommand(Query, sqlConnection);
            using SqlDataReader reader = command.ExecuteReader();
            var Result = new List<T>();
            while (reader.Read())
            {
                var FeildCount = reader.FieldCount;
                var Record = new T();
                var Schema = reader.GetColumnSchema();
                for (int i = 0; i < FeildCount; i++)
                {
                    ObjectDescriber<T, object>.SetValue(Record, Schema[i].ColumnName, reader.GetValue(i));
                }
                Result.Add(Record);
            }
            reader.Close();
            sqlConnection.Close();
            return Result;
        }

        public  void Execute(string Query)
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
