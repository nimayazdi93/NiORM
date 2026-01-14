using System.Linq.Expressions;

namespace NiORM.SQLServer.Interfaces
{
    public interface IWhereEntities<T> where T : ITable
    {
        IEntities<T> Entities { get; set; }
        string WhereConditions { get; set; }
        List<T> ToList();
        void Set(Expression<Func<T, bool>> predicate, object value);
    }
}
