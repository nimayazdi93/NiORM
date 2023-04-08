using NiORM.Mongo.Core;
using NiORM.SQLServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiORM.Mongo
{
    public class DataCore
    {
        private string ConnectionString { get; init; }
        private string DatabaseName { get; init; }
        public DataCore(string ConnectionString, string DatabaseName)
        {
            this.ConnectionString = ConnectionString;
            this.DatabaseName = DatabaseName;
        }

        [Obsolete]
        public DataCore()
        {
            //Do Not Use!!!!
        }

        public Entities<T> CreateEntity<T>() where T : MongoCollection, new()
        {
            return new Entities<T>(ConnectionString,DatabaseName);
        }
    }
}
