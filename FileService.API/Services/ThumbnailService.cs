using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileService.API.Services
{
    // Service responsible for generating and serving file thumbnails
    public class ThumbnailService
    {
        private readonly string _thumbnailFolder;
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };

        public ThumbnailService(IWebHostEnvironment environment)
        {
            // Store thumbnails in 'Uploads/Thumbnails' folder
            _thumbnailFolder = Path.Combine(environment.ContentRootPath, "Uploads", "Thumbnails");

            if (!Directory.Exists(_thumbnailFolder))
            {
                Directory.CreateDirectory(_thumbnailFolder);
            }
        }

        // Checks if the file extension represents an image
        public bool IsImageFile(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ImageExtensions.Contains(ext);
        }

        // Generates or stores a thumbnail image for uploaded image files
        public async Task<string?> GenerateThumbnailAsync(IFormFile file, string storedFileName)
        {
            if (!IsImageFile(file.FileName))
            {
                // Non-image files do not have custom image thumbnails
                return null;
            }

            string thumbFileName = $"thumb_{storedFileName}";
            string thumbPath = Path.Combine(_thumbnailFolder, thumbFileName);

            // Copy file to thumbnail folder
            using (var stream = new FileStream(thumbPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return thumbFileName;
        }

        // Retrieves stream for thumbnail download
        public FileStream? GetThumbnailStream(string thumbFileName)
        {
            string fullPath = Path.Combine(_thumbnailFolder, thumbFileName);
            if (!File.Exists(fullPath))
            {
                return null;
            }

            return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        // Deletes thumbnail image file
        public void DeleteThumbnail(string? thumbFileName)
        {
            if (string.IsNullOrEmpty(thumbFileName)) return;

            string fullPath = Path.Combine(_thumbnailFolder, thumbFileName);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
