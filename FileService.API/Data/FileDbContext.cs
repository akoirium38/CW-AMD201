using FileService.API.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace FileService.API.Data
{
    /// <summary>
    /// FileDbContext provides the MongoDB Atlas database context layer for FileService.API.
    /// Replaces traditional Entity Framework SQL DbContext with MongoDB C# Driver IMongoCollection API.
    /// 
    /// 🔗 Architecture Links:
    /// - Database Connection: Reads "MongoDB:ConnectionString" and "MongoDB:DatabaseName" from appsettings.json
    /// - Collection: Exposes IMongoCollection<FileRecord> pointing to the "files" collection in MongoDB Atlas
    /// - Dependents: Injected into FileService.cs and UploadLimitService.cs
    /// </summary>
    public class FileDbContext
    {
        private readonly IMongoDatabase? _database;

        /// <summary>
        /// Parameterless constructor required by Moq for unit testing (FileServiceUnitTest.cs).
        /// </summary>
        public FileDbContext() { }

        /// <summary>
        /// Main constructor used by ASP.NET Core Dependency Injection container.
        /// Receives singleton IMongoClient and reads database name from appsettings.json.
        /// </summary>
        /// <param name="mongoClient">Connected MongoDB client instance</param>
        /// <param name="configuration">Accesses appsettings.json configuration</param>
        public FileDbContext(IMongoClient mongoClient, IConfiguration configuration)
        {
            // Connect to database name configured in appsettings.json (e.g., "FileServiceDB")
            string dbName = configuration["MongoDB:DatabaseName"] ?? "FileServiceDB";
            _database = mongoClient.GetDatabase(dbName);
        }

        /// <summary>
        /// Constructor overload accepting IConfiguration directly (creates client from ConnectionString).
        /// </summary>
        public FileDbContext(IConfiguration configuration)
        {
            string connectionString = configuration["MongoDB:ConnectionString"] ?? "mongodb://localhost:27017";
            string dbName = configuration["MongoDB:DatabaseName"] ?? "FileServiceDB";
            
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(dbName);
        }

        /// <summary>
        /// Exposes the "files" collection in MongoDB Atlas for CRUD queries.
        /// </summary>
        public virtual IMongoCollection<FileRecord> Files =>
            _database!.GetCollection<FileRecord>("files");
    }
}
