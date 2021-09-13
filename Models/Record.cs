using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Emille.Models
{
    public class Record
    {
        [JsonIgnore]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("key")]
        [JsonProperty("key")]
        public string Key { get; set; } = "";
        
        [BsonElement("value")]
        [JsonProperty("value")]
        public string Value { get; set; } = "";
        
        [JsonProperty("timestamp")]
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UnixEpoch;
    }
}