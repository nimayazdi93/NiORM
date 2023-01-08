using NiORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiORM.Core
{
    public class Table : ITable
    {
        public virtual string TableName => "";

        public virtual List<string> PrimaryKeys => new List<string>();
    }
}
