namespace NiORM.SQLServer.Interfaces
{
    public interface IEntities<T> where T : ITable
    {
        List<T> List();
        T AddReturn(T entity);
        void Add(T entity); 
        void Edit(T entity);
        void Remove(T entity);
    }
}