namespace NiORM.SQLServer.Interfaces
{
    public interface IEntities<T> where T : ITable
    {
        List<T> List();
        int Add(T entity);
        void Edit(T entity);
        void Remove(T entity);
    }
}
