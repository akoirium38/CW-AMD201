using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileService.API.Services
{
    /// <summary>
    /// ThumbnailService manages thumbnail image generation, storage, streaming, and cleanup for uploaded images.
    /// 
    /// 🔗 Architecture Links:
    /// - Caller: FileService.cs calls GenerateThumbnailAsync during UploadFileAsync
    /// - Storage Path: Saved on server disk in "{ContentRoot}/Uploads/Thumbnails"
    /// - Endpoint: Streamed via FilesController.GetThumbnail(string id) -> GET /api/files/{id}/thumbnail
    /// </summary>
    public class ThumbnailService
    {
        private readonly string _thumbnailFolder = string.Empty;
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };

        /// <summary>
        /// Parameterless constructor required by Moq unit testing framework.
        /// </summary>
        public ThumbnailService() { }

        /// <summary>
        /// Dependency Injection Constructor. Initializes "{ContentRoot}/Uploads/Thumbnails" directory.
        /// </summary>
        public ThumbnailService(IWebHostEnvironment environment)
        {
            _thumbnailFolder = Path.Combine(environment.ContentRootPath, "Uploads", "Thumbnails");

            if (!Directory.Exists(_thumbnailFolder))
            {
                Directory.CreateDirectory(_thumbnailFolder);
            }
        }

        /// <summary>
        /// Checks whether the file extension belongs to supported image formats (.jpg, .png, .webp, etc.).
        /// </summary>
        public virtual bool IsImageFile(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ImageExtensions.Contains(ext);
        }

        /// <summary>
        /// Saves a thumbnail copy of the uploaded image file if the file is an image format.
        /// Returns thumbnail filename or null for non-image files (e.g. PDF, DOCX, ZIP).
        /// </summary>
        public virtual async Task<string?> GenerateThumbnailAsync(IFormFile file, string storedFileName)
        {
            if (!IsImageFile(file.FileName))
            {
                // Non-image files do not produce thumbnail image files
                return null;
            }

            string thumbFileName = $"thumb_{storedFileName}";
            string thumbPath = Path.Combine(_thumbnailFolder, thumbFileName);

            // Stream image copy to local thumbnail folder
            using (var stream = new FileStream(thumbPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return thumbFileName;
        }

        /// <summary>
        /// Opens a read-only FileStream for serving thumbnail images to HTTP clients.
        /// </summary>
        public virtual FileStream? GetThumbnailStream(string thumbFileName)
        {
            string fullPath = Path.Combine(_thumbnailFolder, thumbFileName);
            if (!File.Exists(fullPath))
            {
                return null;
            }

            return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        /// <summary>
        /// Deletes the thumbnail file from server disk when the file record is deleted.
        /// </summary>
        public virtual void DeleteThumbnail(string? thumbFileName)
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
