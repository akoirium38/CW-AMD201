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

        // Optional expiration in number of days from upload date
        public int? ExpiryDays { get; set; }

        // Optional maximum number of allowed downloads
        public int? DownloadLimit { get; set; }
    }
}
