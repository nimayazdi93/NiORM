namespace NiORM.Attributes
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class TableName : Attribute
    {
        public TableName(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
