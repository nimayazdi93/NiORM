using NiORM.SQLServer.Interfaces;
using System.Linq.Expressions;

namespace NiORM.SQLServer.Core
{
    internal class WhereEntities<T> : IWhereEntities<T> where T : ITable
    {
        public WhereEntities(IEntities<T> enities, string whereConditions)
        {
            WhereConditions = whereConditions;
            Entities = enities;
        }
        public string WhereConditions { get; set; } = null;
        public IEntities<T> Entities { get; set; } = null;

        public void Set(Expression<Func<T, bool>> predicate, object Value)
        {
            Entities.Set(predicate, Value, WhereConditions);
        }

        public List<T> ToList()
        {
            return Entities.ToList(WhereConditions);
        }
    }
}
