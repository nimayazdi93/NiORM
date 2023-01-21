namespace NiORM.Attributes
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class PrimaryKey : Attribute
    {
        public PrimaryKey() { }
    }
}
