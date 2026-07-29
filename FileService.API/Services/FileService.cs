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
    // Main Service containing core CRUD logic, password verification, expiration, and download tracking
    // Now uses MongoDB Atlas instead of SQL Server
    public class FileService
    {
        private readonly FileDbContext _dbContext;
        private readonly StorageService _storageService;
        private readonly ThumbnailService _thumbnailService;
        private readonly UploadLimitService _uploadLimitService;

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

        // Standard SHA256 password hashing helper function
        private static string HashPassword(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        // Converts a FileRecord MongoDB document to a FileRecordResponseDto for the frontend
        // Field names match the TypeScript FileRecord interface in fe/src/types/file.ts
        public static FileRecordResponseDto MapToDto(FileRecord record)
        {
            return new FileRecordResponseDto
            {
                FileId = record.Id,                          // MongoDB ObjectId string
                FileName = record.FileName,
                ContentType = record.ContentType,
                Size = record.FileSizeBytes,                 // "size" as expected by frontend
                UploadDate = record.UploadDate,
                HasPassword = !string.IsNullOrEmpty(record.PasswordHash),
                ExpiryDate = record.ExpiryDate,
                DownloadLimit = record.DownloadLimit,
                DownloadCount = record.DownloadCount,
                DownloadUrl = $"/api/files/download/{record.Id}",
                ThumbnailUrl = !string.IsNullOrEmpty(record.ThumbnailPath) ? $"/api/files/{record.Id}/thumbnail" : null
            };
        }

        // Uploads a file, saves disk content, thumbnail, and stores database metadata in MongoDB
        public async Task<FileRecordResponseDto> UploadFileAsync(int userId, UploadFileRequestDto dto)
        {
            var file = dto.File;

            // 1. Check single file size limit
            var (isSingleValid, singleMsg) = _uploadLimitService.ValidateSingleFileSize(file.Length);
            if (!isSingleValid)
            {
                throw new InvalidOperationException(singleMsg);
            }

            // 2. Check user storage quota from MongoDB
            var (canUpload, quotaMsg) = await _uploadLimitService.CheckUserQuotaAsync(userId, file.Length);
            if (!canUpload)
            {
                throw new InvalidOperationException(quotaMsg);
            }

            // 3. Save physical file to disk
            var (storedFileName, fullPath) = await _storageService.SaveFileAsync(file);

            // 4. Generate thumbnail if image file
            string? thumbFileName = await _thumbnailService.GenerateThumbnailAsync(file, storedFileName);

            // 5. Parse optional expiry date string from frontend (e.g. "2026-08-15")
            DateTime? expiryDate = null;
            if (!string.IsNullOrWhiteSpace(dto.ExpiryDate) &&
                DateTime.TryParse(dto.ExpiryDate, out DateTime parsedDate))
            {
                expiryDate = parsedDate.ToUniversalTime();
            }

            // 6. Optional password hashing
            string? passwordHash = !string.IsNullOrWhiteSpace(dto.Password)
                ? HashPassword(dto.Password)
                : null;

            // 7. Create the MongoDB document
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

            // Insert document into MongoDB "files" collection
            await _dbContext.Files.InsertOneAsync(fileRecord);

            return MapToDto(fileRecord);
        }

        // Returns all files owned by the specified user, newest first
        public async Task<IEnumerable<FileRecordResponseDto>> GetUserFilesAsync(int userId)
        {
            // MongoDB filter: find all documents where userId matches
            var filter = Builders<FileRecord>.Filter.Eq(f => f.UserId, userId);

            // Sort by upload date descending (newest first)
            var sort = Builders<FileRecord>.Sort.Descending(f => f.UploadDate);

            var files = await _dbContext.Files.Find(filter).Sort(sort).ToListAsync();

            return files.Select(MapToDto);
        }

        // Gets file details by MongoDB ObjectId string
        public async Task<FileRecordResponseDto?> GetFileByIdAsync(string fileId)
        {
            var filter = Builders<FileRecord>.Filter.Eq(f => f.Id, fileId);
            var fileRecord = await _dbContext.Files.Find(filter).FirstOrDefaultAsync();
            if (fileRecord == null) return null;

            return MapToDto(fileRecord);
        }

        // Verifies password for protected file access
        public async Task<bool> VerifyPasswordAsync(string fileId, string rawPassword)
        {
            var filter = Builders<FileRecord>.Filter.Eq(f => f.Id, fileId);
            var fileRecord = await _dbContext.Files.Find(filter).FirstOrDefaultAsync();

            if (fileRecord == null || string.IsNullOrEmpty(fileRecord.PasswordHash))
            {
                return true; // No password protection set
            }

            string inputHash = HashPassword(rawPassword);
            return fileRecord.PasswordHash.Equals(inputHash, StringComparison.OrdinalIgnoreCase);
        }

        // Validates constraints and retrieves physical file stream for download
        public async Task<(FileStream? Stream, string ContentType, string FileName, string? ErrorMessage)> PrepareDownloadAsync(string fileId, string? password)
        {
            var filter = Builders<FileRecord>.Filter.Eq(f => f.Id, fileId);
            var fileRecord = await _dbContext.Files.Find(filter).FirstOrDefaultAsync();

            if (fileRecord == null)
            {
                return (null, string.Empty, string.Empty, "File not found.");
            }

            // Check expiry date
            if (fileRecord.ExpiryDate.HasValue && DateTime.UtcNow > fileRecord.ExpiryDate.Value)
            {
                return (null, string.Empty, string.Empty, "This file link has expired.");
            }

            // Check download limit
            if (fileRecord.DownloadLimit.HasValue && fileRecord.DownloadCount >= fileRecord.DownloadLimit.Value)
            {
                return (null, string.Empty, string.Empty, "This file has reached its maximum download limit.");
            }

            // Check password protection
            if (!string.IsNullOrEmpty(fileRecord.PasswordHash))
            {
                if (string.IsNullOrEmpty(password) || !await VerifyPasswordAsync(fileId, password))
                {
                    return (null, string.Empty, string.Empty, "Incorrect or missing password for this file.");
                }
            }

            // Retrieve disk stream
            var stream = _storageService.GetFileStream(fileRecord.StoredFileName);
            if (stream == null)
            {
                return (null, string.Empty, string.Empty, "File binary content not found on server disk.");
            }

            // Increment download count in MongoDB using an atomic update
            var update = Builders<FileRecord>.Update.Inc(f => f.DownloadCount, 1);
            await _dbContext.Files.UpdateOneAsync(filter, update);

            return (stream, fileRecord.ContentType, fileRecord.FileName, null);
        }

        // Deletes a file record from MongoDB and cleans up disk files
        public async Task<bool> DeleteFileAsync(string fileId, int userId)
        {
            // Only delete if the file belongs to this user (security check)
            var filter = Builders<FileRecord>.Filter.And(
                Builders<FileRecord>.Filter.Eq(f => f.Id, fileId),
                Builders<FileRecord>.Filter.Eq(f => f.UserId, userId)
            );

            var fileRecord = await _dbContext.Files.Find(filter).FirstOrDefaultAsync();
            if (fileRecord == null)
            {
                return false;
            }

            // Delete physical stored file from disk
            _storageService.DeleteFile(fileRecord.StoredFileName);

            // Delete thumbnail if present
            _thumbnailService.DeleteThumbnail(fileRecord.ThumbnailPath);

            // Remove the document from MongoDB
            await _dbContext.Files.DeleteOneAsync(filter);

            return true;
        }

        // Gets stream for thumbnail display
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
