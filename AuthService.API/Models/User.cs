using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuthService.API.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Gmail { get; set; }
        
        public string Password { get; set; }
    }
}
