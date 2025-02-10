using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiORM.SQLServer.Core
{
    /// <summary>
    /// For Describe Features Of PrimaryKey
    /// </summary>
    public class PrimaryKeyDetails
    {
        public PrimaryKeyDetails(string name, bool isAutoIncremental=true, bool isGUID=false )
        {
            Name = name;
            IsAutoIncremental = isAutoIncremental;
            IsGUID = isGUID;
        }

        public string Name { get; set; }
        public bool IsAutoIncremental { get; set; } 
        public bool IsGUID { get; set; }
    }
}
