using System;

namespace FileService.API.DTOs
{
    // Response DTO containing file details sent back to the frontend SPA.
    // Field names match the TypeScript FileRecord interface in fe/src/types/file.ts
    public class FileRecordResponseDto
    {
        // "fileId" matches TypeScript interface field (was: Id)
        public string FileId { get; set; } = string.Empty;

        // "fileName" matches TypeScript interface field
        public string FileName { get; set; } = string.Empty;

        // "size" matches TypeScript interface field (was: FileSizeBytes)
        public long Size { get; set; }

        // File MIME type (e.g. "application/pdf", "image/png")
        public string ContentType { get; set; } = string.Empty;

        public DateTime UploadDate { get; set; }

        // Whether this file requires a password to download
        public bool HasPassword { get; set; }

        // "expiryDate" matches TypeScript interface field
        public DateTime? ExpiryDate { get; set; }

        public int? DownloadLimit { get; set; }

        // "downloadCount" matches TypeScript interface field
        public int DownloadCount { get; set; }

        // Direct URL to download this file
        public string DownloadUrl { get; set; } = string.Empty;

        // URL to the thumbnail image (only for image files)
        public string? ThumbnailUrl { get; set; }
    }
}
