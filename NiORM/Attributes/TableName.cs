namespace NiORM.Attributes
{
    /// <summary>
    /// Attribute to specify the database table name for an entity class
    /// </summary>
    /// <remarks>
    /// This attribute is used to map entity classes to specific database table names.
    /// If not specified, the class name will be used as the table name.
    /// </remarks>
    /// <example>
    /// <code>
    /// [TableName("Users")]
    /// public class User : ITable
    /// {
    ///     // Properties...
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class TableName : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the TableName attribute
        /// </summary>
        /// <param name="name">The name of the database table</param>
        /// <exception cref="ArgumentNullException">Thrown when name is null or empty</exception>
        public TableName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name), "Table name cannot be null or empty");
            
            Name = name;
        }

        /// <summary>
        /// Gets or sets the database table name
        /// </summary>
        public string Name { get; set; }
    }
}
