using NiORM.Core;
using NiORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiORM
{
    public class DataCore
    {
        private string ConnectionString { get; set; }
        public DataCore(string ConnectionString)
        {
            this.ConnectionString = ConnectionString;
        }
        public DataCore()
        {
            //Do Not Use!!!!
        }
         
        public   Entities<T> CreateEntity<T>() where T : ITable, new()
        {
            return new Entities<T>(ConnectionString);
        }
       

    }
}
