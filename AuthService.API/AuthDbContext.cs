using AuthService.API.Models;
using MongoDB.Driver;

namespace AuthService.API
{
    public class AuthDbContext
    {
        private readonly IMongoDatabase _database;

        public AuthDbContext(
            IMongoClient mongoClient,
            IConfiguration configuration)
        {
            string dbName =
                configuration["MongoDB:DatabaseName"] ?? "AuthDBContext";

            _database = mongoClient.GetDatabase(dbName);
        }

        // Users collection
        public IMongoCollection<User> Users =>
            _database.GetCollection<User>("users");

        // OTP codes collection
        public IMongoCollection<OtpCode> OtpCodes =>
            _database.GetCollection<OtpCode>("otpCodes");
    }
}