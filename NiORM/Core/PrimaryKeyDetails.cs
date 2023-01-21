using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiORM.Core
{
    /// <summary>
    /// For Describe Features Of PrimaryKey
    /// </summary>
    public class PrimaryKeyDetails
    {
        public PrimaryKeyDetails(string name, bool isAutoIncremental)
        {
            Name = name;
            IsAutoIncremental = isAutoIncremental;
        }

        public string Name { get; set; }
        public bool IsAutoIncremental { get; set; }
    }
}
