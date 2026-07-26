namespace FileService.API.Models
{
    // Represents a file stored in the database with metadata, password protection, and expiry limits
    public class FileRecord
    {
        // Primary key ID for the file record
        public int Id { get; set; }

        // ID of the user who uploaded the file (retrieved from JWT claim)
        public int UserId { get; set; }

        // Original file name uploaded by user (e.g., "report.pdf")
        public string FileName { get; set; } = string.Empty;

        // Unique file name on disk to prevent overwriting (e.g., "guid_report.pdf")
        public string StoredFileName { get; set; } = string.Empty;

        // MIME type of the file (e.g., "application/pdf", "image/png")
        public string ContentType { get; set; } = string.Empty;

        // Size of file in bytes
        public long FileSizeBytes { get; set; }

        // Date and time when the file was uploaded
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        // Optional password hash if the file is password-protected
        public string? PasswordHash { get; set; }

        // Optional expiration date after which the file cannot be accessed
        public DateTime? ExpiryDate { get; set; }

        // Optional limit on how many times the file can be downloaded
        public int? DownloadLimit { get; set; }

        // Current count of how many times the file has been downloaded
        public int DownloadCount { get; set; } = 0;

        // Optional path to the generated thumbnail image (for image files)
        public string? ThumbnailPath { get; set; }
    }
}
