using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuthService.API.Models
{
    public class OtpCode
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; }
    }
}
