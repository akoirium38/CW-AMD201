using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FileService.API.Services
{
    // Simple service to handle physical disk file upload, retrieval, and deletion
    public class StorageService
    {
        private readonly string _uploadFolder = string.Empty;

        // Parameterless constructor for Moq unit testing
        public StorageService() { }

        public StorageService(IWebHostEnvironment environment)
        {
            // Store uploaded files inside 'Uploads' folder in the web root or content root
            _uploadFolder = Path.Combine(environment.ContentRootPath, "Uploads");

            // Ensure the directory exists
            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }
        }

        // Saves an incoming file to disk with a unique GUID prefix
        public virtual async Task<(string StoredFileName, string FullPath)> SaveFileAsync(IFormFile file)
        {
            // Generate unique stored file name to avoid collisions (e.g. "a1b2c3d4_myDocument.pdf")
            string uniquePrefix = Guid.NewGuid().ToString("N");
            string safeFileName = Path.GetFileName(file.FileName);
            string storedFileName = $"{uniquePrefix}_{safeFileName}";
            string fullPath = Path.Combine(_uploadFolder, storedFileName);

            // Copy binary data to local file stream
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return (storedFileName, fullPath);
        }

        // Retrieves a file stream for downloading
        public virtual FileStream? GetFileStream(string storedFileName)
        {
            string fullPath = Path.Combine(_uploadFolder, storedFileName);
            if (!File.Exists(fullPath))
            {
                return null;
            }

            return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        // Deletes a file from physical disk storage
        public virtual bool DeleteFile(string storedFileName)
        {
            string fullPath = Path.Combine(_uploadFolder, storedFileName);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
            return false;
        }

        // Returns physical folder path
        public virtual string GetUploadFolder() => _uploadFolder;
    }
}
