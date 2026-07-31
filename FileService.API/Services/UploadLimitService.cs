using FileService.API.Data;
using FileService.API.DTOs;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FileService.API.Services
{
    /// <summary>
    /// UploadLimitService calculates storage quota limits per user and validates file upload sizes.
    /// 
    /// 🔗 Architecture Links:
    /// - Database: Queries MongoDB Atlas ("files" collection) via FileDbContext to sum total uploaded file sizes per UserId.
    /// - Quota Rule: Enforces a default 100 MB max quota per user and 50 MB max single file size.
    /// - Frontend Dashboard: Powers the GET /api/files/storage-quota endpoint used for displaying storage usage progress bars in React UI.
    /// </summary>
    public class UploadLimitService
    {
        private readonly FileDbContext? _dbContext;

        // Default maximum storage quota per user: 100 MB (104,857,600 bytes)
        public const long MaxStoragePerUserBytes = 100 * 1024 * 1024;

        // Maximum allowed file size for a single upload: 50 MB (52,428,800 bytes)
        public const long MaxSingleFileSizeBytes = 50 * 1024 * 1024;

        /// <summary>
        /// Parameterless constructor required for Moq unit testing framework.
        /// </summary>
        public UploadLimitService() { }

        /// <summary>
        /// Dependency Injection Constructor.
        /// </summary>
        public UploadLimitService(FileDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Validates if an incoming file exceeds the 50 MB single file upload limit.
        /// </summary>
        /// <param name="fileSizeBytes">Size of incoming file in bytes</param>
        /// <returns>Tuple (IsValid, ValidationErrorMessage)</returns>
        public virtual (bool IsValid, string Message) ValidateSingleFileSize(long fileSizeBytes)
        {
            if (fileSizeBytes <= 0)
            {
                return (false, "File cannot be empty.");
            }

            if (fileSizeBytes > MaxSingleFileSizeBytes)
            {
                return (false, $"File size exceeds maximum allowed single file upload limit of {MaxSingleFileSizeBytes / (1024 * 1024)} MB.");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Checks whether uploading a new file would cause the user to exceed their 100 MB total storage quota.
        /// Queries MongoDB Atlas to calculate the sum of all existing file sizes owned by the user.
        /// </summary>
        /// <param name="userId">Target user's ID</param>
        /// <param name="newFileSizeBytes">Size of the new file being uploaded</param>
        /// <returns>Tuple (CanUpload, QuotaErrorMessage)</returns>
        public virtual async Task<(bool CanUpload, string Message)> CheckUserQuotaAsync(int userId, long newFileSizeBytes)
        {
            if (_dbContext == null) return (true, string.Empty);

            // Query MongoDB Atlas collection for all file records owned by this user
            var filter = Builders<Models.FileRecord>.Filter.Eq(f => f.UserId, userId);
            var userFiles = await _dbContext.Files.Find(filter).ToListAsync();

            // Calculate total bytes currently stored in MongoDB Atlas for this user
            long usedBytes = userFiles.Sum(f => f.FileSizeBytes);

            // Reject upload if total size will exceed 100 MB limit
            if (usedBytes + newFileSizeBytes > MaxStoragePerUserBytes)
            {
                double remainingMB = Math.Max(0, Math.Round((double)(MaxStoragePerUserBytes - usedBytes) / (1024 * 1024), 2));
                return (false, $"Upload exceeds your storage quota limit (100 MB). Available remaining storage: {remainingMB} MB.");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Computes storage usage report (used bytes, max capacity 100MB, file count) for the logged-in user.
        /// Used by FilesController.GetStorageQuota() to return data to React frontend.
        /// </summary>
        /// <param name="userId">Logged-in User ID</param>
        /// <returns>StorageQuotaDto object</returns>
        public virtual async Task<StorageQuotaDto> GetUserStorageQuotaAsync(int userId)
        {
            if (_dbContext == null)
            {
                return new StorageQuotaDto { UsedBytes = 0, MaxBytes = MaxStoragePerUserBytes, FileCount = 0 };
            }

            // Query MongoDB Atlas for user's files
            var filter = Builders<Models.FileRecord>.Filter.Eq(f => f.UserId, userId);
            var userFiles = await _dbContext.Files.Find(filter).ToListAsync();

            long usedBytes = userFiles.Sum(f => f.FileSizeBytes);
            int count = userFiles.Count;

            return new StorageQuotaDto
            {
                UsedBytes = usedBytes,
                MaxBytes = MaxStoragePerUserBytes,
                FileCount = count
            };
        }
    }
}
