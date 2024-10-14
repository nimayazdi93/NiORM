using NiORM.Mongo;
using NiORM.SQLServer.Core;
using NiORM.SQLServer.Interfaces;

namespace NiORM.SQLServer
{
    public class DataCore : IDataCore
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

        public IEntities<T> CreateEntity<T>() where T : ITable, new()
        {
            return new Entities<T>(ConnectionString);
        }

        public List<T> SqlRaw<T>(string Query)  
        {
            SqlMaster<T> sqlMaster = new(ConnectionString);
            List<T> result = sqlMaster.Get(Query);
            return result;
        }


    }
}
