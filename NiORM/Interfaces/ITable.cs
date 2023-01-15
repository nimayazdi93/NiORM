using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NiORM.Interfaces
{
    public interface ITable
    {
        //[JsonIgnore]
        //string TableName { get; }
        //[JsonIgnore]
        //List<string> PrimaryKeys { get; }
    }

    public interface IView : ITable
    {

    }
}
