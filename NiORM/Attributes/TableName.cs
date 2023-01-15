using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiORM.Attributes
{
    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class TableName:System.Attribute
    {
        public TableName(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
