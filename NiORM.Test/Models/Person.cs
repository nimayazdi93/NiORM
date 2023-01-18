using NiORM.Attributes;
using NiORM.Core;
using NiORM.Interfaces;

namespace NiORM.Test.Models
{
    [TableName("People")]
    public class Person : ITable
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }  

    }
}