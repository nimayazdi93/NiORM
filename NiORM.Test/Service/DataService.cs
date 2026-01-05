
using NiORM.SQLServer;
using NiORM.SQLServer.Interfaces;
using NiORM.Test.Models;

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

        private IEntities<Person> _people;
        public IEntities<Person> People { get { _people ??= CreateEntity<Person>(); return _people; } }

    }
}
