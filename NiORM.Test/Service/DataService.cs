
using NiORM.SQLServer;
using NiORM.SQLServer.Core;
using NiORM.SQLServer.Interfaces;
using NiORM.Test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiORM.Test.Service
{
    public class DataService : DataCore
    {
        /// <summary>
        /// For Reading ConnectionString From Config File
        /// </summary>
        public DataService() : base("") { }
        /// <summary>
        /// For Using Many Databases with One DataCore
        /// </summary>
        /// <param name="ConnectionString"></param>
        public DataService(string ConnectionString) : base(ConnectionString) { }

        public IEntities<Person> People => CreateEntity<Person>();

    }
}
