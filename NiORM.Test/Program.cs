using NiORM.Test.Models;
using NiORM.Test.Service;
Console.WriteLine("First implement DataService with your models and set connection string");
Console.WriteLine("\n\n");
DataService dataService = new DataService();

//for fetch data from DB
var people = dataService.People.List();



//for add a new person
var person = new Person() { Age = 29, Id = 1, Name = "Nima" };
dataService.People.Add(person);



//for edit the person
person.Age = 30;
dataService.People.Edit(person);




//for Remove the person
dataService.People.Remove(person);