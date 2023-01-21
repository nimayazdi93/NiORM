namespace NiORM.Attributes
{
    /// <summary>
    /// To Assing A Property As PrimaryKey 
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class PrimaryKey : Attribute
    {

        public PrimaryKey() { }
        /// <summary>
        /// For Assing A Property with Auto Increment
        /// </summary>
        /// <param name="isAutoIncremental">To Assign That PrimaryKey is Auto Incremental or not, Default=TRUE</param>
        public PrimaryKey(bool  isAutoIncremental=true)
        {
            IsAutoIncremental = isAutoIncremental;
        }

        public bool IsAutoIncremental { get; set; } = true;
    }
}
