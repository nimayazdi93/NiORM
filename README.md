# NiORM
 **NiORM** is a lightweight Object-Relational Mapper (ORM) for .NET, designed to simplify interactions with SQL databases. It uses a convention-over-configuration approach, enabling developers to map C# classes to database tables using attributes. NiORM offers an intuitive interface for querying and manipulating data with minimal overhead.

## Features
- **Attribute-based Mapping**: Use `[TableName]`, `[PrimaryKey]`, and other annotations to define the schema directly in your C# classes.
- **Entity Management**: Create, retrieve, update, and delete records easily through `Entities<T>` collections.
- **Query Simplification**: Chain LINQ-style queries for simple and advanced data filtering.
- **Raw SQL Execution**: Execute raw SQL queries when needed, returning mapped objects.
- **Multiple Database Support**: Handle multiple databases within the same project.
- **🔐 SQL Injection Protection**: All methods use parameterized queries by default for maximum security.
- **🆕 Security Validation**: Automatic detection and prevention of SQL injection attacks.
- **🆕 Comprehensive Error Handling**: Custom exception classes for better error identification and handling.
- **🆕 Built-in Logging System**: Configurable logging with multiple levels and output options.
- **🆕 Enhanced XML Documentation**: Complete IntelliSense support with detailed examples and exception documentation.
- **🆕 Enhanced LINQ Support**: Improved DateTime operations including `.Date` property support and nullable DateTime handling (v1.6.0-beta+).

## Installation
Download & Install the nuget using:

Nuget Package Manager:

```NuGet\Install-Package NiORM -Version```

.Net CLI:

```dotnet add package NiORM```



## Quick Start
Here’s how you can get started with NiORM in your application:

#### 1. Define a Model:

Use attributes like ```[TableName]``` and ```[PrimaryKey]``` to map a ```C#``` class to a database table.

```
using NiORM.Attributes;
using NiORM.SQLServer.Interfaces;

[TableName("People")]
public class Person : ITable
{
    [PrimaryKey(isAutoIncremental: true)]
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
}
```
#### 2. Set up a Data Service:

Create a service class that inherits from DataCore. This class will act as the interface between your application and the database.

```
using NiORM.SQLServer;
using NiORM.Test.Models;

public class DataService : DataCore
{
    public DataService(string connectionString) : base(connectionString) { }

    public IEntities<Person> People => CreateEntity<Person>();
}
```
#### 3. Interact with the Database (SQL Injection Safe):

Use the data service to fetch, insert, update, and delete records. All operations are protected against SQL injection!

```
var dataService = new DataService("your-connection-string-here");

// Fetch all people
var people = dataService.People.ToList();

// Add a new person (safe!)
var newPerson = new Person() { Age = 29, Name = "John'; DROP TABLE Users; --" };
dataService.People.Add(newPerson); // This is completely safe!

// Safe queries with parameterized WHERE conditions
var filteredPeople = dataService.People.Where(p => p.Name == "Nima").ToList();

// Safe multiple conditions
var conditions = new Dictionary<string, object?>
{
    { "Name", "John'; DROP TABLE Users; --" }, // Safe!
    { "Age", 25 }
};
var safePeople = dataService.People.WhereMultiple(conditions);

// Safe property search
var byName = dataService.People.FindByProperty("Name", userInput); // Always safe!

// Date comparison (NEW in v1.6.0-beta)
var todayPeople = dataService.People.Where(p => p.DateTime.Value.Date == DateTime.Now.Date).ToList();
```
#### 4. Execute Raw SQL (when necessary - with security warnings):

If you need more control, you can execute raw SQL queries. NiORM will automatically validate and warn about potential security issues.

```
// ⚠️ Raw SQL (automatically validated for injection attempts)
var cats = dataService.SqlRaw<Cat>("SELECT * FROM Cats");

// ✅ Better: Use safe parameterized approach
var paramHelper = new SqlParameterHelper();
var nameParam = paramHelper.AddParameter("Fluffy");
var safeCats = dataService.SqlRaw<Cat>($"SELECT * FROM Cats WHERE Name = {nameParam}");
```

**🚨 Important**: Always prefer safe methods like `FindByProperty()`, `WhereMultiple()`, and LINQ expressions over raw SQL!

## 🔐 Security & Error Handling

NiORM v1.6.0-beta+ includes comprehensive SQL injection protection, error handling, and security logging:

#### Enable Logging:

```csharp
using NiORM.SQLServer.Core;

// Enable logging to console
NiORMLogger.IsEnabled = true;
NiORMLogger.MinimumLogLevel = LogLevel.Info;

// Or log to file
NiORMLogger.LogFilePath = @"C:\logs\niorm.log";
```

#### Enhanced Error Handling:

```csharp
try
{
    var person = dataService.People.Find(123);
}
catch (NiORMValidationException ex)
{
    // Handle validation errors
    Console.WriteLine($"Validation error: {ex.Message}");
}
catch (NiORMConnectionException ex)
{
    // Handle connection issues
    Console.WriteLine($"Connection failed: {ex.Message}");
}
catch (NiORMException ex)
{
    // Handle other NiORM errors
    Console.WriteLine($"Database error: {ex.Message}");
    if (!string.IsNullOrEmpty(ex.SqlQuery))
    {
        Console.WriteLine($"Failed query: {ex.SqlQuery}");
    }
}
```

#### Safe Operations with Enhanced Service:

```csharp
public class SafeDataService : DataCore
{
    public SafeDataService(string connectionString) : base(connectionString)
    {
        // Configure logging
        NiORMLogger.IsEnabled = true;
        NiORMLogger.MinimumLogLevel = LogLevel.Warning;
    }

    public bool TryAddPerson(Person person, out string errorMessage)
    {
        errorMessage = string.Empty;
        try
        {
            People.Add(person);
            return true;
        }
        catch (NiORMException ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    public IEntities<Person> People => CreateEntity<Person>();
}
```

📖 **For detailed documentation, see:** 
- [🔐 SQL Injection Protection Guide](./NiORM/SQLServer/SQL_INJECTION_PROTECTION_GUIDE.md) - **Essential reading for secure usage!**
- [Error Handling and Logging Guide](./NiORM/SQLServer/ERROR_HANDLING_AND_LOGGING_GUIDE.md)

## 📦 Version History

### 🆕 Version 1.6.0-beta (Pre-Release)

**⚠️ Note:** This is a pre-release version. Use with caution in production environments.

#### Changes:
- **Updated project version to 1.6.0-beta**
- **Enhanced Security Features:**
  - Improved SQL injection protection with comprehensive validation
  - Automatic detection and prevention of SQL injection attacks
- **Enhanced Logging System:**
  - Improved logging for entity operations
  - Detailed error messages with better type handling
  - Enhanced logging context for debugging
- **Better Error Handling:**
  - Updated method signatures to return nullable types for better error handling
  - More descriptive error messages
  - Improved exception handling throughout the codebase
- **LINQ Expression Improvements:**
  - Fixed handling of DateTime properties (e.g., `.Date` property)
  - Improved support for nullable DateTime operations
  - Better handling of nested member access expressions

#### Installation:
```bash
dotnet add package NiORM --version 1.6.0-beta
```

Or via NuGet Package Manager:
```
Install-Package NiORM -Version 1.6.0-beta
```

## Contributing
We welcome contributions! Please fork the repository and submit pull requests for any improvements or features you'd like to add.

## License
This project is licensed under the MIT License.
