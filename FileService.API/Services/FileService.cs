using FileService.API.Data;
using FileService.API.DTOs;
using FileService.API.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileService.API.Services
{
    /// <summary>
    /// FileService is the core business logic layer responsible for handling file uploads,
    /// MongoDB document mapping, SHA256 password security, expiration checks, download counting, and deletion.
    /// 
    /// 🔗 Architecture Links:
    /// - Database: Interacts with MongoDB Atlas collection ("files") via FileDbContext.cs
    /// - Cloud Storage: Calls StorageService.cs to stream physical files to Firebase Cloud Storage
    /// - Thumbnail Generator: Calls ThumbnailService.cs to generate image previews
    /// - Quota Validation: Calls UploadLimitService.cs to enforce per-user storage caps
    /// - Frontend DTO Mapping: MapToDto converts MongoDB documents into DTOs matching fe/src/types/file.ts
    /// </summary>
    public class FileService
    {
        private readonly FileDbContext _dbContext;
        private readonly StorageService _storageService;
        private readonly ThumbnailService _thumbnailService;
        private readonly UploadLimitService _uploadLimitService;

        /// <summary>
        /// Constructor injected by ASP.NET Core Dependency Injection system.
        /// </summary>
        public FileService(
            FileDbContext dbContext,
            StorageService storageService,
            ThumbnailService thumbnailService,
            UploadLimitService uploadLimitService)
        {
            _dbContext = dbContext;
            _storageService = storageService;
            _thumbnailService = thumbnailService;
            _uploadLimitService = uploadLimitService;
        }

        /// <summary>
        /// Hashes raw plaintext passwords using SHA256 cryptographic hashing.
        /// Converts the output byte array to a 64-character uppercase hexadecimal string.
        /// Example: "123456" -> "8D969EEF6ECAD3C29A3A629280E686CF0C3F5D5A86AFF3CA12020C923ADC6C92"
        /// </summary>
        private static string HashPassword(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        /// <summary>
        /// Utility method mapping a MongoDB FileRecord document into a FileRecordResponseDto object
        /// expected by the React frontend (fe/src/types/file.ts).
        /// </summary>
        /// <param name="record">MongoDB document</param>
        /// <returns>Frontend-compatible response DTO</returns>
        public static FileRecordResponseDto MapToDto(FileRecord record)
        {
            return new FileRecordResponseDto
            {
                FileId = record.Id,                          // MongoDB ObjectId string
                FileName = record.FileName,
                ContentType = record.ContentType,
                Size = record.FileSizeBytes,                 // File size in bytes ("size" property in React)
                UploadDate = record.UploadDate,
                HasPassword = !string.IsNullOrEmpty(record.PasswordHash), // True if file is password protected
                ExpiryDate = record.ExpiryDate,
                DownloadLimit = record.DownloadLimit,
                DownloadCount = record.DownloadCount,
                DownloadUrl = $"/api/files/download/{record.Id}",
                ThumbnailUrl = !string.IsNullOrEmpty(record.ThumbnailPath) ? $"/api/files/{record.Id}/thumbnail" : null
            };
        }

        /// <summary>
        /// Core Upload Workflow:
        /// 1. Validates single file size limit (UploadLimitService)
        /// 2. Validates total user storage quota in MongoDB Atlas (UploadLimitService)
        /// 3. Streams file binary to Firebase Storage cloud bucket (StorageService)
        /// 4. Generates optional PNG thumbnail for image files (ThumbnailService)
        /// 5. Hashes optional protection password with SHA256
        /// 6. Saves file record document into MongoDB Atlas ("files" collection)
        /// </summary>
        public async Task<FileRecordResponseDto> UploadFileAsync(int userId, UploadFileRequestDto dto)
        {
            var file = dto.File;

            // Step 1: Check single file size limit (e.g. 50 MB max)
            var (isSingleValid, singleMsg) = _uploadLimitService.ValidateSingleFileSize(file.Length);
            if (!isSingleValid)
            {
                throw new InvalidOperationException(singleMsg);
            }

            // Step 2: Check total storage quota for user in MongoDB Atlas
            var (canUpload, quotaMsg) = await _uploadLimitService.CheckUserQuotaAsync(userId, file.Length);
            if (!canUpload)
            {
                throw new InvalidOperationException(quotaMsg);
            }

            // Step 3: Upload physical file binary to Firebase Cloud Storage (or local disk fallback)
            var (storedFileName, fullPath) = await _storageService.SaveFileAsync(file);

            // Step 4: Generate thumbnail preview image if file is an image (JPG, PNG, WEBP)
            string? thumbFileName = await _thumbnailService.GenerateThumbnailAsync(file, storedFileName);

            // Step 5: Parse optional expiration date string (e.g. "2026-12-31")
            DateTime? expiryDate = null;
            if (!string.IsNullOrWhiteSpace(dto.ExpiryDate) &&
                DateTime.TryParse(dto.ExpiryDate, out DateTime parsedDate))
            {
                expiryDate = parsedDate.ToUniversalTime();
            }

            // Step 6: Hash optional password with SHA256 (e.g., "123456")
            string? passwordHash = !string.IsNullOrWhiteSpace(dto.Password)
                ? HashPassword(dto.Password)
                : null;

            // Step 7: Instantiate new MongoDB FileRecord document
            var fileRecord = new FileRecord
            {
                UserId = userId,
                FileName = file.FileName,
                StoredFileName = storedFileName,
                ContentType = file.ContentType ?? "application/octet-stream",
                FileSizeBytes = file.Length,
                UploadDate = DateTime.UtcNow,
                PasswordHash = passwordHash,
                ExpiryDate = expiryDate,
                DownloadLimit = dto.DownloadLimit,
                DownloadCount = 0,
                ThumbnailPath = thumbFileName
            };

            // Step 8: Insert document into MongoDB Atlas "files" collection
            await _dbContext.Files.InsertOneAsync(fileRecord);

            return MapToDto(fileRecord);
        }

        /// <summary>
        /// Retrieves all file records owned by a specific user from MongoDB Atlas, ordered newest-first.
        /// </summary>
        public async Task<IEnumerable<FileRecordResponseDto>> GetUserFilesAsync(int userId)
        {
            // Filter MongoDB documents matching UserId
            var filter = Builders<FileRecord>.Filter.Eq(f => f.UserId, userId);

            // Sort by UploadDate descending (newest uploads first)
            var sort = Builders<FileRecord>.Sort.Descending(f => f.UploadDate);

            var files = await _dbContext.Files.Find(filter).Sort(sort).ToListAsync();

            return files.Select(MapToDto);
        }

        /// <summary>
        /// Finds a specific file document by MongoDB ObjectId string (e.g. "64a1f2b3c4d5e6f7a8b9c0d1").
        /// </summary>
        public async Task<FileRecordResponseDto?> GetFileByIdAsync(string fileId)
        {
            var filter = Builders<FileRecord>.Filter.Eq(f => f.Id, fileId);
            var fileRecord = await _dbContext.Files.Find(filter).FirstOrDefaultAsync();
            if (fileRecord == null) return null;

            return MapToDto(fileRecord);
        }

        /// <summary>
        /// Verifies a user-supplied plaintext password against the SHA256 hash stored in MongoDB Atlas.
        /// </summary>
        public async Task<bool> VerifyPasswordAsync(string fileId, string rawPassword)
        {
            var filter = Builders<FileRecord>.Filter.Eq(f => f.Id, fileId);
            var fileRecord = await _dbContext.Files.Find(filter).FirstOrDefaultAsync();

            // If file record doesn't exist or has no password, verification passes
            if (fileRecord == null || string.IsNullOrEmpty(fileRecord.PasswordHash))
            {
                return true;
            }

            // Hash incoming user input and compare with stored hash
            string inputHash = HashPassword(rawPassword);
            return fileRecord.PasswordHash.Equals(inputHash, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Validates expiration date, download limits, and password protection before preparing file stream for download.
        /// Atomically increments MongoDB download count upon successful download.
        /// </summary>
        public async Task<(Stream? Stream, string ContentType, string FileName, string? ErrorMessage)> PrepareDownloadAsync(string fileId, string? password)
        {
            var filter = Builders<FileRecord>.Filter.Eq(f => f.Id, fileId);
            var fileRecord = await _dbContext.Files.Find(filter).FirstOrDefaultAsync();

            if (fileRecord == null)
            {
                return (null, string.Empty, string.Empty, "File not found.");
            }

            // 1. Validate Expiration Date
            if (fileRecord.ExpiryDate.HasValue && DateTime.UtcNow > fileRecord.ExpiryDate.Value)
            {
                return (null, string.Empty, string.Empty, "This file link has expired.");
            }

            // 2. Validate Maximum Download Limit
            if (fileRecord.DownloadLimit.HasValue && fileRecord.DownloadCount >= fileRecord.DownloadLimit.Value)
            {
                return (null, string.Empty, string.Empty, "This file has reached its maximum download limit.");
            }

            // 3. Validate Password Security
            if (!string.IsNullOrEmpty(fileRecord.PasswordHash))
            {
                if (string.IsNullOrEmpty(password) || !await VerifyPasswordAsync(fileId, password))
                {
                    return (null, string.Empty, string.Empty, "Incorrect or missing password for this file.");
                }
            }

            // 4. Retrieve Physical File Stream from local storage or cloud
            var stream = await _storageService.GetFileStreamAsync(fileRecord.StoredFileName);
            if (stream == null)
            {
                return (null, string.Empty, string.Empty, "File binary content not found on server disk or cloud storage.");
            }

            // 5. Increment Download Count in MongoDB Atlas atomically ($inc operation)
            var update = Builders<FileRecord>.Update.Inc(f => f.DownloadCount, 1);
            await _dbContext.Files.UpdateOneAsync(filter, update);

            return (stream, fileRecord.ContentType, fileRecord.FileName, null);
        }

        /// <summary>
        /// Deletes file metadata document from MongoDB Atlas and purges stored object from Firebase Cloud Storage.
        /// Enforces user ownership security check (UserId matching).
        /// </summary>
        public async Task<bool> DeleteFileAsync(string fileId, int userId)
        {
            var filter = Builders<FileRecord>.Filter.And(
                Builders<FileRecord>.Filter.Eq(f => f.Id, fileId),
                Builders<FileRecord>.Filter.Eq(f => f.UserId, userId)
            );

            var fileRecord = await _dbContext.Files.Find(filter).FirstOrDefaultAsync();
            if (fileRecord == null)
            {
                return false;
            }

            // Delete physical stored object from Firebase Cloud Storage / disk
            _storageService.DeleteFile(fileRecord.StoredFileName);

            // Delete thumbnail file if present
            _thumbnailService.DeleteThumbnail(fileRecord.ThumbnailPath);

            // Delete document record from MongoDB Atlas
            await _dbContext.Files.DeleteOneAsync(filter);

            return true;
        }

        /// <summary>
        /// Updates the metadata (FileName, Password, ExpiryDate) of an existing file record in MongoDB Atlas.
        /// Verifies user ownership security (userId) before saving updates.
        /// </summary>
        /// <param name="fileId">MongoDB ObjectId string of the target file</param>
        /// <param name="userId">Logged-in user ID extracted from JWT token</param>
        /// <param name="dto">Update DTO containing new metadata values</param>
        /// <returns>Updated FileRecordResponseDto or null if file not found / unauthorized</returns>
        public async Task<FileRecordResponseDto?> UpdateFileMetadataAsync(string fileId, int userId, UpdateFileRequestDto dto)
        {
            // Step 1: Find file record in MongoDB matching fileId AND userId (ownership check)
            var filter = Builders<FileRecord>.Filter.And(
                Builders<FileRecord>.Filter.Eq(f => f.Id, fileId),
                Builders<FileRecord>.Filter.Eq(f => f.UserId, userId)
            );

            var fileRecord = await _dbContext.Files.Find(filter).FirstOrDefaultAsync();

            // If file does not exist or user doesn't own it, return null
            if (fileRecord == null)
            {
                return null;
            }

            // Step 2: Update FileName if a new name is provided
            if (!string.IsNullOrWhiteSpace(dto.FileName))
            {
                string originalExt = Path.GetExtension(fileRecord.FileName);
                string newName = dto.FileName.Trim();
                
                // Preserve original extension if it's missing from the new name
                if (!string.IsNullOrEmpty(originalExt) && !newName.EndsWith(originalExt, StringComparison.OrdinalIgnoreCase))
                {
                    newName += originalExt;
                }
                
                fileRecord.FileName = newName;
            }

            // Step 3: Update Password Hash
            if (dto.Password != null)
            {
                if (dto.Password == string.Empty)
                {
                    // Empty string means remove password protection
                    fileRecord.PasswordHash = null;
                }
                else if (!string.IsNullOrWhiteSpace(dto.Password))
                {
                    // Hash new password using SHA256
                    fileRecord.PasswordHash = HashPassword(dto.Password);
                }
            }

            // Step 4: Update ExpiryDate
            if (dto.ExpiryDate != null)
            {
                if (dto.ExpiryDate == string.Empty)
                {
                    // Empty string means clear expiry date
                    fileRecord.ExpiryDate = null;
                }
                else if (DateTime.TryParse(dto.ExpiryDate, out DateTime parsedDate))
                {
                    fileRecord.ExpiryDate = parsedDate.ToUniversalTime();
                }
            }

            // Step 5: Update DownloadLimit / MaxDownloads
            if (dto.DownloadLimit.HasValue || dto.MaxDownloads.HasValue)
            {
                fileRecord.DownloadLimit = dto.DownloadLimit ?? dto.MaxDownloads;
            }

            // Step 6: Replace updated document in MongoDB Atlas database collection
            await _dbContext.Files.ReplaceOneAsync(filter, fileRecord);

            // Step 7: Return updated DTO for response
            return MapToDto(fileRecord);
        }

        /// <summary>
        /// Updates access control and security settings (Password, ExpiryDate, DownloadLimit) of an existing file.
        /// Reuses UpdateFileMetadataAsync logic to avoid code duplication.
        /// </summary>
        public async Task<FileRecordResponseDto?> UpdateFileAccessAsync(string fileId, int userId, UpdateFileRequestDto dto)
        {
            return await UpdateFileMetadataAsync(fileId, userId, dto);
        }

        /// <summary>
        /// Streams thumbnail image file stream for display on frontend file grid UI.
        /// </summary>
        public async Task<FileStream?> GetThumbnailStreamAsync(string fileId)
        {
            var filter = Builders<FileRecord>.Filter.Eq(f => f.Id, fileId);
            var fileRecord = await _dbContext.Files.Find(filter).FirstOrDefaultAsync();

            if (fileRecord == null || string.IsNullOrEmpty(fileRecord.ThumbnailPath))
            {
                return null;
            }

            return _thumbnailService.GetThumbnailStream(fileRecord.ThumbnailPath);
        }
    }
}
