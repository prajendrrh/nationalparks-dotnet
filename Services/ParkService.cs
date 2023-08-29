using NationalParks.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using System.Collections.Generic;
using System.Linq;

using System;
using System.IO;

namespace NationalParks.Services
{
    public class ParkService
    {
        private readonly IMongoCollection<Park> _parks;

        public ParkService(INationalparksDatabaseSettings settings)
        {
            var mongoDbServerHost = Environment.GetEnvironmentVariable("MONGODB_SERVER_HOST") ?? "127.0.0.1";
            var mongoDbServerPort = Environment.GetEnvironmentVariable("MONGODB_SERVER_PORT") ?? "27017";
            var mongoDbDatabase = Environment.GetEnvironmentVariable("MONGODB_DATABASE") ?? "mongodb";
            var mongoDbUser = Environment.GetEnvironmentVariable("MONGODB_USER") ?? "mongodb";
            var mongoDbPassword = Environment.GetEnvironmentVariable("MONGODB_PASSWORD") ?? "mongodb";

            var connectionString = $"mongodb://{mongoDbUser}:{mongoDbPassword}@{mongoDbServerHost}:{mongoDbServerPort}/{mongoDbDatabase}";

            Console.WriteLine($"Connecting with {connectionString} to database {mongoDbDatabase}");

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(mongoDbDatabase);

            _parks = database.GetCollection<Park>(settings.ParksCollectionName);
        }

        public List<Park> Get() =>
            _parks.Find(Park => true).ToList();

        public Park Get(string id) =>
            _parks.Find<Park>(Park => Park.Id == id).FirstOrDefault();

        public Park Create(Park Park)
        {
            _parks.InsertOne(Park);
            return Park;
        }

        public string Load()
        {
            string line;
            int i = 0;
            using (TextReader file = File.OpenText(@"nationalparks.json"))
            {
                while ((line = file.ReadLine()) != null)
                {
                    var bsonDocument = BsonDocument.Parse(line);
                    var myObj = BsonSerializer.Deserialize<Park>(bsonDocument);
                    _parks.InsertOneAsync(myObj);
                    i++;
                }
            }
            return "Items inserted in database: " + i;

        }

        public void Update(string id, Park ParkIn) =>
            _parks.ReplaceOne(Park => Park.Id == id, ParkIn);

        public void Remove(Park ParkIn) =>
            _parks.DeleteOne(Park => Park.Id == ParkIn.Id);

        public void Remove(string id) => 
            _parks.DeleteOne(Park => Park.Id == id);
    }
}