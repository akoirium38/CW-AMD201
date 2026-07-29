using Microsoft.AspNetCore.Http;

namespace FileService.API.DTOs
{
    // Request DTO sent by frontend when uploading a new file
    public class UploadFileRequestDto
    {
        // The binary file content uploaded via multipart form-data
        public required IFormFile File { get; set; }

        // Optional password to protect the file from unauthorized downloads
        public string? Password { get; set; }

        // Optional expiration date string sent by frontend (e.g. "2026-08-15")
        // Changed from ExpiryDays (int) to ExpiryDate (string) to match frontend form field
        public string? ExpiryDate { get; set; }

        // Optional maximum number of allowed downloads
        public int? DownloadLimit { get; set; }
    }
}
