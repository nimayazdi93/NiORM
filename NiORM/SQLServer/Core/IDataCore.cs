using NiORM.Mongo.Interfaces;
using NiORM.SQLServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiORM.SQLServer.Core
{
    public interface IDataCore
    {
        Interfaces.IEntities<T> CreateEntity<T>() where T :ITable, new();
        List<T> SqlRaw<T>(string Query);
    }
}
