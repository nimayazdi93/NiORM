namespace NiORM.Interfaces
{
    public interface IEntities<T> where T : ITable
    {
        List<T> List();
        void Add(T entity);
        void Edit(T entity);
        void Remove(T entity);
    }
}
