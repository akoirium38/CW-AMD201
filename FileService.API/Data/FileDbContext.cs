using FileService.API.Models;
using MongoDB.Driver;

namespace FileService.API.Data
{
    // MongoDB context — replaces the old EF Core FileDbContext
    // This class provides access to the "files" collection in MongoDB Atlas
    public class FileDbContext
    {
        private readonly IMongoDatabase _database;

        public FileDbContext(IMongoClient mongoClient, IConfiguration configuration)
        {
            // Connect to the specific database named in appsettings.json (e.g. "FileServiceDB")
            string dbName = configuration["MongoDB:DatabaseName"] ?? "FileServiceDB";
            _database = mongoClient.GetDatabase(dbName);
        }

        // Provides access to the "files" collection in MongoDB
        // This is equivalent to the old DbSet<FileRecord> Files property
        public IMongoCollection<FileRecord> Files =>
            _database.GetCollection<FileRecord>("files");
    }
}
