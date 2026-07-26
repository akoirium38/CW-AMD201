using FileService.API.Data;
using FileService.API.DTOs;
using FileService.API.Models;
using Microsoft.EntityFrameworkCore;
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

        // Converts a FileRecord database entity to a FileRecordResponseDto for frontend
        public static FileRecordResponseDto MapToDto(FileRecord record)
        {
            return new FileRecordResponseDto
            {
                Id = record.Id,
                FileName = record.FileName,
                ContentType = record.ContentType,
                FileSizeBytes = record.FileSizeBytes,
                UploadDate = record.UploadDate,
                HasPassword = !string.IsNullOrEmpty(record.PasswordHash),
                ExpiryDate = record.ExpiryDate,
                DownloadLimit = record.DownloadLimit,
                DownloadCount = record.DownloadCount,
                DownloadUrl = $"/api/files/{record.Id}/download",
                ThumbnailUrl = !string.IsNullOrEmpty(record.ThumbnailPath) ? $"/api/files/{record.Id}/thumbnail" : null
            };
        }

        // Uploads a file, saves disk content, thumbnail, and stores database metadata record
        public async Task<FileRecordResponseDto> UploadFileAsync(int userId, UploadFileRequestDto dto)
        {
            var file = dto.File;

            // 1. Check single file size limit
            var (isSingleValid, singleMsg) = _uploadLimitService.ValidateSingleFileSize(file.Length);
            if (!isSingleValid)
            {
                throw new InvalidOperationException(singleMsg);
            }

            // 2. Check user storage quota
            var (canUpload, quotaMsg) = await _uploadLimitService.CheckUserQuotaAsync(userId, file.Length);
            if (!canUpload)
            {
                throw new InvalidOperationException(quotaMsg);
            }

            // 3. Save physical file to disk
            var (storedFileName, fullPath) = await _storageService.SaveFileAsync(file);

            // 4. Generate thumbnail if image file
            string? thumbFileName = await _thumbnailService.GenerateThumbnailAsync(file, storedFileName);

            // 5. Calculate optional expiry date
            DateTime? expiryDate = dto.ExpiryDays.HasValue && dto.ExpiryDays.Value > 0
                ? DateTime.UtcNow.AddDays(dto.ExpiryDays.Value)
                : null;

            // 6. Optional password hashing
            string? passwordHash = !string.IsNullOrWhiteSpace(dto.Password)
                ? HashPassword(dto.Password)
                : null;

            // 7. Create entity record
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

            // Save to database
            _dbContext.Files.Add(fileRecord);
            await _dbContext.SaveChangesAsync();

            return MapToDto(fileRecord);
        }

        // Returns all files owned by the specified user
        public async Task<IEnumerable<FileRecordResponseDto>> GetUserFilesAsync(int userId)
        {
            var files = await _dbContext.Files
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.UploadDate)
                .ToListAsync();

            return files.Select(MapToDto);
        }

        // Gets file details by file ID
        public async Task<FileRecordResponseDto?> GetFileByIdAsync(int fileId)
        {
            var fileRecord = await _dbContext.Files.FindAsync(fileId);
            if (fileRecord == null) return null;

            return MapToDto(fileRecord);
        }

        // Verifies password for protected file access
        public async Task<bool> VerifyPasswordAsync(int fileId, string rawPassword)
        {
            var fileRecord = await _dbContext.Files.FindAsync(fileId);
            if (fileRecord == null || string.IsNullOrEmpty(fileRecord.PasswordHash))
            {
                return true; // No password protection set
            }

            string inputHash = HashPassword(rawPassword);
            return fileRecord.PasswordHash.Equals(inputHash, StringComparison.OrdinalIgnoreCase);
        }

        // Validates constraints and retrieves physical file stream for download
        public async Task<(FileStream? Stream, string ContentType, string FileName, string? ErrorMessage)> PrepareDownloadAsync(int fileId, string? password)
        {
            var fileRecord = await _dbContext.Files.FindAsync(fileId);
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

            // Increment download count
            fileRecord.DownloadCount++;
            await _dbContext.SaveChangesAsync();

            return (stream, fileRecord.ContentType, fileRecord.FileName, null);
        }

        // Deletes a file record from DB and cleans up disk files
        public async Task<bool> DeleteFileAsync(int fileId, int userId)
        {
            var fileRecord = await _dbContext.Files.FirstOrDefaultAsync(f => f.Id == fileId && f.UserId == userId);
            if (fileRecord == null)
            {
                return false;
            }

            // Delete physical stored file
            _storageService.DeleteFile(fileRecord.StoredFileName);

            // Delete thumbnail if present
            _thumbnailService.DeleteThumbnail(fileRecord.ThumbnailPath);

            // Remove database record
            _dbContext.Files.Remove(fileRecord);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        // Gets stream for thumbnail display
        public async Task<FileStream?> GetThumbnailStreamAsync(int fileId)
        {
            var fileRecord = await _dbContext.Files.FindAsync(fileId);
            if (fileRecord == null || string.IsNullOrEmpty(fileRecord.ThumbnailPath))
            {
                return null;
            }

            return _thumbnailService.GetThumbnailStream(fileRecord.ThumbnailPath);
        }
    }
}
