using MongoDB.Driver;
using NiORM.Attributes;
using NiORM.Mongo.Interfaces; 
namespace NiORM.Mongo.Core
{
    public class Entities<T> : Interfaces.IEntities<T> where T : MongoCollection, new()
    {  
        private string GetCollectionName()
        {
            T entity = new T();
            if (entity is null) throw new ArgumentNullException(nameof(entity));
            var entityType = entity.GetType();

            try
            {
                var attributes = Attribute.GetCustomAttributes(entityType);
                foreach (var attribute in attributes)
                {
                    if (attribute is CollectionName)
                    {
                        return ((CollectionName)attribute).Name;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            throw new Exception($"class '{entityType.Name}' should have attribute 'CollectionName'");
        }
        public string CollectionName => GetCollectionName();
        private MongoClient client { get; set; } 
        private IMongoDatabase mongodatabase { get; set; }
        private IMongoCollection<T> mongoCollection { get; set; }
        public Entities(string connectionString, string databaseName)
        {
            client = new MongoClient(connectionString);

            mongodatabase = client.GetDatabase(databaseName);
            mongoCollection = mongodatabase.GetCollection<T>(CollectionName);
        }
        public bool Add(T entity)
        {
            mongoCollection.InsertOne(entity);
            return true;
        }

        public   bool Edit(T item)
        {
            var result = mongoCollection.ReplaceOne(t => t.ID == item.ID, item);
            if (result.ModifiedCount > 0)
                return true;
            return false;
        }

        public List<T> List()
        {
            var Items = mongoCollection.Find(FilterDefinition<T>.Empty);
            return Items.ToList();
        }
        public  List<T> Get(string Query)
        {
            var Items = mongoCollection.Find(Query);
            return Items.ToList();
        }
        public T Find(string ID)
        {
            var item = mongoCollection.Find(t => t.ID == ID);
            return item.FirstOrDefault();
        }

        public bool Remove(T entity)
        {
            var result = mongoCollection.DeleteOne(t => t.ID == entity.ID);
            if (result.DeletedCount > 0)
                return true;

            return false;
        }
        public bool Remove(string ID)
        {
            var result = mongoCollection.DeleteOne(t => t.ID == ID);
            if (result.DeletedCount > 0)
                return true;

            return false;
        }
    }
}
