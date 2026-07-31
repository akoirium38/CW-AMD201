using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace FileService.API.Models
{
    /// <summary>
    /// FileRecord represents the BSON document schema stored inside MongoDB Atlas ("files" collection).
    /// BSON attributes map C# class properties to MongoDB database document fields.
    /// 
    /// 🔗 Architecture Links:
    /// - Database Collection: Maps to MongoDB Atlas database "FileServiceDB", collection "files".
    /// - Data Access: Handled via MongoDB C# Driver inside FileDbContext.cs and FileService.cs.
    /// - Frontend DTO: Converted into FileRecordResponseDto by FileService.MapToDto for React UI.
    /// </summary>
    public class FileRecord
    {
        /// <summary>
        /// Unique MongoDB Document ID (_id).
        /// Stored as a 24-character hexadecimal ObjectId in Atlas (e.g., "64a1f2b3c4d5e6f7a8b9c0d1").
        /// Exposed as string in C# for seamless JSON serialization.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        /// <summary>
        /// ID of the user who owns this file.
        /// Extracted from JWT token ClaimTypes.NameIdentifier issued by AuthService.API.
        /// </summary>
        [BsonElement("userId")]
        public int UserId { get; set; }

        /// <summary>
        /// Original filename uploaded by user (e.g., "document.pdf").
        /// Displayed in React frontend FileHub UI list.
        /// </summary>
        [BsonElement("fileName")]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Unique object key stored in Firebase Storage cloud bucket or server disk (e.g., "a3e2227c_document.pdf").
        /// Prevents overwriting files if multiple users upload files with identical names.
        /// </summary>
        [BsonElement("storedFileName")]
        public string StoredFileName { get; set; } = string.Empty;

        /// <summary>
        /// MIME content type of file (e.g. "application/pdf", "image/png", "application/zip").
        /// </summary>
        [BsonElement("contentType")]
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// Total file size in bytes (e.g., 204800 bytes = 200 KB).
        /// </summary>
        [BsonElement("fileSizeBytes")]
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// UTC Timestamp recorded when the file was uploaded.
        /// </summary>
        [BsonElement("uploadDate")]
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// SHA256 hashed password string if file is password-protected (null if public).
        /// Raw passwords (e.g. "123456") are hashed before saving to MongoDB Atlas.
        /// </summary>
        [BsonElement("passwordHash")]
        public string? PasswordHash { get; set; }

        /// <summary>
        /// Expiration UTC timestamp after which the file cannot be accessed or downloaded.
        /// </summary>
        [BsonElement("expiryDate")]
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Maximum download count allowed for this file link (null if unlimited).
        /// </summary>
        [BsonElement("downloadLimit")]
        public int? DownloadLimit { get; set; }

        /// <summary>
        /// Counter tracking total times this file has been downloaded.
        /// Incremented atomically in MongoDB Atlas ($inc) upon download.
        /// </summary>
        [BsonElement("downloadCount")]
        public int DownloadCount { get; set; } = 0;

        /// <summary>
        /// Relative path to generated thumbnail preview file (for image files).
        /// </summary>
        [BsonElement("thumbnailPath")]
        public string? ThumbnailPath { get; set; }
    }
}
