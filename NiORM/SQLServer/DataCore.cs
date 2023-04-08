using NiORM.SQLServer.Core;
using NiORM.SQLServer.Interfaces;

namespace NiORM.SQLServer
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

        public Entities<T> CreateEntity<T>() where T : ITable, new()
        {
            return new Entities<T>(ConnectionString);
        }
    }
}
