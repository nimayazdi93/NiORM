namespace NiORM.Attributes
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class CollectionName : Attribute
    {
        public CollectionName(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
