# NiORM
 **NiORM** is a lightweight Object-Relational Mapper (ORM) for .NET, designed to simplify interactions with SQL databases. It uses a convention-over-configuration approach, enabling developers to map C# classes to database tables using attributes. NiORM offers an intuitive interface for querying and manipulating data with minimal overhead.

## Features
- **Attribute-based Mapping**: Use `[TableName]`, `[PrimaryKey]`, and other annotations to define the schema directly in your C# classes.
- **Entity Management**: Create, retrieve, update, and delete records easily through `Entities<T>` collections.
- **Query Simplification**: Chain LINQ-style queries for simple and advanced data filtering.
- **Raw SQL Execution**: Execute raw SQL queries when needed, returning mapped objects.
- **Multiple Database Support**: Handle multiple databases within the same project.

## Installation
Download & Install the nuget using:

Nuget Package Manager:

```NuGet\Install-Package NiORM -Version 1.3.2```

.Net CLI:

```dotnet add package NiORM --version 1.3.2```



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

    public Entities<Person> People => CreateEntity<Person>();
}
```
#### 3. Interact with the Database:

Use the data service to fetch, insert, update, and delete records.

```
var dataService = new DataService("your-connection-string-here");

// Fetch all people
var people = dataService.People.ToList();

// Add a new person
var newPerson = new Person() { Age = 29, Name = "Nima" };
dataService.People.Add(newPerson);

// Query with conditions
var filteredPeople = dataService.People.Where(p => p.Name == "Nima").ToList();
```
#### 4. Execute Raw SQL (when necessary):

If you need more control, you can execute raw SQL queries and map them to your models.

```
var cats = dataService.SqlRaw<Cat>("SELECT * FROM Cats");
```

## Contributing
We welcome contributions! Please fork the repository and submit pull requests for any improvements or features you'd like to add.

## License
This project is licensed under the MIT License.
