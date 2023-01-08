using NiORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiORM.Core
{
    /// <summary>
    /// An object for CRUD actions
    /// </summary>
    /// <typeparam name="T">Type Of Object related to table. It should be inherited from ITable</typeparam>
    public class Entities<T> where T : ITable, new()
    {
        private string ConnectionString { get; set; }
        private SQLMaster<T> SQLMaster { get; set; } 
        public Entities(string ConnectionString)
        {
            this.ConnectionString = ConnectionString;
            SQLMaster = new SQLMaster<T>(ConnectionString);
        }
        /// <summary>
        /// A method for first row in table
        /// </summary>
        /// <returns></returns>
        public T First() =>SQLMaster.Get(Query: $"select top(1) * from {new T().TableName}").FirstOrDefault();
        /// <summary>
        ///   A method for first row in table with conditions in TSQL
        /// </summary>
        /// <param name="Query">TSQL Query</param>
        /// <returns></returns>
        public T First(string Query) =>SQLMaster.Get($"select top(1) * from {new T().TableName} where {Query}").FirstOrDefault();
        /// <summary>
        /// A method for find an object using its primary key (Just for tables with one PK)
        /// </summary>
        /// <param name="ID">Primary Key</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T Find(string ID)
        {
            var Keys = new T().PrimaryKeys;
            if (Keys.Count != 1)
                throw new Exception("The count of arguments are not same as PrimaryKeys");
            var Entity = new T();
            ObjectDescriber<T, int>.SetValue(Entity, Keys[0], int.Parse(ID));
            return SQLMaster.Get($"Select top(1) * from {new T().TableName} where  [{Keys[0]}]= {ObjectDescriber<T, string>.SQLFormat(Entity, Keys[0])}").FirstOrDefault();
        }
        /// <summary>
        /// A method for find an object using its primary key (Just for tables with one PK)
        /// </summary>
        /// <param name="ID">Primary Key</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T Find(int ID) => Find(ID.ToString());
        /// <summary>
        /// A method for find an object using its primary key (Just for tables with two PK)
        /// </summary>
        /// <param name="ID1">Primary Key</param>
        /// <param name="ID2">Primary Key</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T Find(string ID1, string ID2)
        {
            var Keys = new T().PrimaryKeys;
            if (Keys.Count != 2)
                throw new Exception("The count of arguments are not same as PrimaryKeys");
            var Entity = new T();
            ObjectDescriber<T, int>.SetValue(Entity, Keys[0], int.Parse(ID1));
            ObjectDescriber<T, int>.SetValue(Entity, Keys[1], int.Parse(ID2));
            return SQLMaster.Get($@"Select top(1) * from {new T().TableName}
                                    where
                                        [{Keys[0]}]= {ObjectDescriber<T, string>.SQLFormat(Entity, Keys[0])}
                                        and
                                         [{Keys[1]}]= {ObjectDescriber<T, string>.SQLFormat(Entity, Keys[1])}").FirstOrDefault();

        }
        /// <summary>
        /// A method for find an object using its primary key (Just for tables with two PK)
        /// </summary>
        /// <param name="ID1">Primary Key</param>
        /// <param name="ID2">Primary Key</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T Find(int ID1, string ID2) => Find(ID1.ToString(), ID2.ToString());
        public List<T> List()
        {
            var Properties = ObjectDescriber<T, string>.Properties(new T());
            var Addition = "";
            if (Properties.Any(c => c == "IsActive"))
            {
                Addition = $"order by IsActive desc,{new T().PrimaryKeys.FirstOrDefault()}";
            }
             
            return SQLMaster.Get($"select * from {new T().TableName} {Addition}").ToList();
        }
        /// <summary>
        /// A method for fetching table with Query in where
        /// </summary>
        /// <param name="Query"></param>
        /// <returns></returns>
        public List<T> Query(string Query)
        {
            return SQLMaster.Get(Query).ToList();
        }
        /// <summary>
        /// A method for executing a TSQL command
        /// </summary>
        /// <param name="Query"></param>
        public void Execute(string Query)
        {
           SQLMaster.Execute(Query);
        }
        /// <summary>
        /// A method for fetching table with Query in where
        /// </summary>
        /// <param name="Query"></param>
        /// <returns></returns>
        public List<T> List(string Query)
        {
            return SQLMaster.Get($"select * from {new T().TableName} where {Query}").ToList();
        }

        public List<T> Where(Func<T, bool> Predict)
        {
            return List().Where(Predict).ToList();
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


        private string GetType()
        {
            return new T().GetType().ToString().Split(".").LastOrDefault();
        }
        /// <summary>
        /// A method for adding a row
        /// </summary>
        /// <param name="entity">object we are adding</param>
        /// <exception cref="Exception"></exception>
        public int Add(T entity)
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
                                        .Properties(entity)
                                        .Where(c => c != "TableName" & c != "PrimaryKeys")
                                        .Where(c => c.Replace("ID", "") != Type).ToList();
            var Query = $@"insert into {entity.TableName} 
                           (
                            {string.Join(",\n", ListOfProperties.Select(c => $"[{c}]").ToList())}
                            )
                            Values
                            (
                             {string.Join(",\n", ListOfProperties.Select(c => ObjectDescriber<T, int>.SQLFormat(entity, c)).ToList())}
                             )";

           SQLMaster.Execute(Query);

            var ID = 0;
            entity = this.Query($"select  {entity.PrimaryKeys.FirstOrDefault()} from {entity.TableName} order by {entity.PrimaryKeys.FirstOrDefault()} desc").FirstOrDefault();
            return ObjectDescriber<T, int>.GetValue(entity, entity.PrimaryKeys.FirstOrDefault());

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
               .Properties(entity)
               .Where(c => c != "TableName").ToList();
            var NonKeys = ListOfProperties.Where(c => c != "PrimaryKeys").Where(c => entity.PrimaryKeys.All(cc => cc != c)).ToList();
            var Query = $@"update {entity.TableName}
                           set {string.Join(",\n", NonKeys.Select(c => $"[{c}]={ObjectDescriber<T, int>.SQLFormat(entity, c)}").ToList())}
                           where {string.Join(" and ", entity.PrimaryKeys.Select(c => $" [{c}]= {ObjectDescriber<T, int>.SQLFormat(entity, c)}").ToList())}";

           SQLMaster.Execute(Query);
        }
        /// <summary>
        /// A method for removing a row
        /// </summary>
        /// <param name="entity">object we are removing</param>
        public void Remove(T entity)
        {
            var Query = $@"delete {entity.TableName}  
                            where {string.Join(" and ", entity.PrimaryKeys.Select(c => $" [{c}]= {ObjectDescriber<T, int>.SQLFormat(entity, c)}").ToList())}";
           SQLMaster.Execute(Query);
        }
    }
}
