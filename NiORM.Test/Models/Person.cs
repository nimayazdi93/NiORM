using NiORM.Core;
using NiORM.Interfaces;

namespace NiORM.Test.Models
{
    public class Person : ITable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public   string TableName => "People";

        public   List<string> PrimaryKeys => new List<string>() { "Id" };

    }
}