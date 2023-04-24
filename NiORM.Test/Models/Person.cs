using NiORM.Attributes; 
using NiORM.SQLServer.Interfaces;

namespace NiORM.Test.Models
{
    [TableName("People")]
    public class Person : ITable
    {
        [PrimaryKey(isAutoIncremental:true)]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }

    }
}