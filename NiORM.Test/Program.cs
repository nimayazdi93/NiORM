using NiORM.Test.Models;
using NiORM.Test.Service;
Console.WriteLine("First implement DataService with your models and set connection string");
Console.WriteLine("\n\n");
DataService dataService = new DataService("Password=<SQL PASSWORD>;Persist Security Info=True;User ID=<SQL USER>;Initial Catalog=<DATABASE NAME>;Data Source=<SQL DATASOURCE>");

//for fetch data from DB
var people = dataService.People.List();



//for add a new person
var person = new Person() { Age = 29,  Name = "Nima" };
dataService.People.Add(person);

person = dataService.People.First();

//for edit the person
person.Age = 30;
dataService.People.Edit(person);




//for Remove the person
dataService.People.Remove(person);