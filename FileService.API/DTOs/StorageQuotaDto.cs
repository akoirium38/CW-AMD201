namespace FileService.API.DTOs
{
    // Response DTO containing storage quota and usage statistics for a user
    public class StorageQuotaDto
    {
        public long UsedBytes { get; set; }
        public long MaxBytes { get; set; }
        public double UsedMB => Math.Round((double)UsedBytes / (1024 * 1024), 2);
        public double MaxMB => Math.Round((double)MaxBytes / (1024 * 1024), 2);
        public int FileCount { get; set; }
        public double UsagePercentage => MaxBytes > 0 ? Math.Round(((double)UsedBytes / MaxBytes) * 100, 2) : 0;
    }
}
