using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FileService.API.Models
{
    // Represents a file document stored in MongoDB Atlas
    // The [BsonId] attribute marks the MongoDB _id field
    // The [BsonElement] attribute maps C# property names to MongoDB field names
    public class FileRecord
    {
        // MongoDB document ID (stored as ObjectId in Atlas, exposed as string)
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        // ID of the user who uploaded the file (retrieved from JWT claim)
        [BsonElement("userId")]
        public int UserId { get; set; }

        // Original file name uploaded by user (e.g., "report.pdf")
        [BsonElement("fileName")]
        public string FileName { get; set; } = string.Empty;

        // Unique file name on disk to prevent overwriting (e.g., "guid_report.pdf")
        [BsonElement("storedFileName")]
        public string StoredFileName { get; set; } = string.Empty;

        // MIME type of the file (e.g., "application/pdf", "image/png")
        [BsonElement("contentType")]
        public string ContentType { get; set; } = string.Empty;

        // Size of file in bytes
        [BsonElement("fileSizeBytes")]
        public long FileSizeBytes { get; set; }

        // Date and time when the file was uploaded (UTC)
        [BsonElement("uploadDate")]
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        // Optional password hash if the file is password-protected
        [BsonElement("passwordHash")]
        public string? PasswordHash { get; set; }

        // Optional expiration date after which the file cannot be accessed
        [BsonElement("expiryDate")]
        public DateTime? ExpiryDate { get; set; }

        // Optional limit on how many times the file can be downloaded
        [BsonElement("downloadLimit")]
        public int? DownloadLimit { get; set; }

        // Current count of how many times the file has been downloaded
        [BsonElement("downloadCount")]
        public int DownloadCount { get; set; } = 0;

        // Optional path to the generated thumbnail image (for image files)
        [BsonElement("thumbnailPath")]
        public string? ThumbnailPath { get; set; }
    }
}
