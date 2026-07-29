using FileService.API.Data;
using FileService.API.DTOs;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace FileService.API.Services
{
    // Service to check upload quotas, user limits, and maximum file sizes
    public class UploadLimitService
    {
        private readonly FileDbContext _dbContext;

        // Default quota per user: 100 MB
        public const long MaxStoragePerUserBytes = 100 * 1024 * 1024;

        // Max single file upload size: 50 MB
        public const long MaxSingleFileSizeBytes = 50 * 1024 * 1024;

        public UploadLimitService(FileDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Validates if incoming file size exceeds single file limit
        public (bool IsValid, string Message) ValidateSingleFileSize(long fileSizeBytes)
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

        // Checks if uploading a new file will exceed the user's total storage quota
        public async Task<(bool CanUpload, string Message)> CheckUserQuotaAsync(int userId, long newFileSizeBytes)
        {
            // Query MongoDB for all files belonging to this user
            var filter = Builders<Models.FileRecord>.Filter.Eq(f => f.UserId, userId);
            var userFiles = await _dbContext.Files.Find(filter).ToListAsync();

            // Sum up total storage used
            long usedBytes = userFiles.Sum(f => f.FileSizeBytes);

            if (usedBytes + newFileSizeBytes > MaxStoragePerUserBytes)
            {
                double remainingMB = Math.Max(0, Math.Round((double)(MaxStoragePerUserBytes - usedBytes) / (1024 * 1024), 2));
                return (false, $"Upload exceeds your storage quota limit (100 MB). Available remaining storage: {remainingMB} MB.");
            }

            return (true, string.Empty);
        }

        // Gets detailed storage quota report for user
        public async Task<StorageQuotaDto> GetUserStorageQuotaAsync(int userId)
        {
            // Query MongoDB for all files owned by this user
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
