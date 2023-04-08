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
        public DataCore(string ConnectionString)
        {
            this.ConnectionString = ConnectionString;
        }

        [Obsolete]
        public DataCore()
        {
            //Do Not Use!!!!
        }

        public Entities<T> CreateEntity<T>() where T : MongoCollection, new()
        {
            return new Entities<T>(ConnectionString);
        }
    }
}
