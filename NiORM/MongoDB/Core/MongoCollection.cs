using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using NiORM.Mongo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NiORM.Mongo.Core
{
    public class MongoCollection : ICollection
    {
        [BsonId, BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        [JsonIgnore]

        public string ID { get; set; }
    }
}
