using System;

namespace FileService.API.DTOs
{
    // Response DTO containing file details sent back to the frontend SPA
    public class FileRecordResponseDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public DateTime UploadDate { get; set; }
        public bool HasPassword { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? DownloadLimit { get; set; }
        public int DownloadCount { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
    }
}
