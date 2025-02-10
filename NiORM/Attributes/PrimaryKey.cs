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
        /// <param name="isGUID">To Assign That PrimaryKey is GUID or not, Default=FALSE</param>

        public PrimaryKey(bool  isAutoIncremental=true, bool isGUID=false)
        {
            IsAutoIncremental = isAutoIncremental;
            IsGUID = isGUID;
        }

        public bool IsAutoIncremental { get; set; } = true;
        public bool IsGUID { get; set; } = false;
    }
}
