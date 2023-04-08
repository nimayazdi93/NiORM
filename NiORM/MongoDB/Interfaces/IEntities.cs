using NiORM.SQLServer.Interfaces;

namespace NiORM.Mongo.Interfaces
{
    public interface IEntities<T> where T : ICollection
    {
        List<T> List();
        bool Add(T entity);
        bool Edit(T entity);
        bool Remove(T entity);
    }
}
