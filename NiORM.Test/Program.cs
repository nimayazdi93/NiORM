using NiORM.Test.Models;
using NiORM.Test.Service;
Console.WriteLine("First implement DataService with your models and set connection string");
Console.WriteLine("\n\n");
DataService dataService = new DataService("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=StoreDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");


//for fetch data from DB
var people = dataService.People.ToList();

//for add a new person
var person = new Person() { Age = 29, Name = "Nima" };
person = dataService.People.Add(person);

person = dataService.People.FirstOrDefault();

people = dataService.People.Where(c => c.Name == "Nima" && c.Age==30).ToList();

//for edit the person
person.Age = 30;
dataService.People.Edit(person);

//for Remove the person
dataService.People.Remove(person);