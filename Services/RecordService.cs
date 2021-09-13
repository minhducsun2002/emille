using System;
using Emille.Models;
using MongoDB.Driver;

namespace Emille.Services
{
    public class RecordService
    {
        public readonly IMongoCollection<Record> Collection;

        public RecordService()
        {
            var client = new MongoClient(Environment.GetEnvironmentVariable("MONGODB_URL"));
            Collection = client.GetDatabase("records").GetCollection<Record>("records");
        }
    }
}