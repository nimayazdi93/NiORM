using NiORM.Interfaces;

namespace NiORM.Core
{
    public class Table : ITable
    {
        public virtual string TableName => string.Empty;

        public virtual List<string> PrimaryKeys => new List<string>();
    }
}
