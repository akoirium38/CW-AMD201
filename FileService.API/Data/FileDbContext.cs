using FileService.API.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace FileService.API.Data
{
    // MongoDB context — replaces the old EF Core FileDbContext
    // This class provides access to the "files" collection in MongoDB Atlas
    public class FileDbContext
    {
        private readonly IMongoDatabase? _database;

        // Parameterless constructor for Moq unit testing
        public FileDbContext() { }

        public FileDbContext(IMongoClient mongoClient, IConfiguration configuration)
        {
            // Connect to the specific database named in appsettings.json (e.g. "FileServiceDB")
            string dbName = configuration["MongoDB:DatabaseName"] ?? "FileServiceDB";
            _database = mongoClient.GetDatabase(dbName);
        }

        // Provides access to the "files" collection in MongoDB
        public virtual IMongoCollection<FileRecord> Files =>
            _database!.GetCollection<FileRecord>("files");
    }
}
