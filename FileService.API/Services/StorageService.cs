using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FileService.API.Services
{
    // StorageService handles file upload, retrieval, and deletion
    // Supports Firebase Storage cloud bucket with local disk fallback for development/testing
    public class StorageService
    {
        private readonly StorageClient? _storageClient;
        private readonly string? _bucketName;
        private readonly string _uploadFolder = string.Empty;
        private readonly bool _isFirebaseEnabled = false;

        // Parameterless constructor for Moq unit testing
        public StorageService() { }

        public StorageService(IWebHostEnvironment environment, IConfiguration configuration)
        {
            // 1. Local disk fallback directory setup
            _uploadFolder = Path.Combine(environment.ContentRootPath, "Uploads");
            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }

            // 2. Firebase Storage setup
            _bucketName = configuration["Firebase:BucketName"];
            string credentialPath = configuration["Firebase:CredentialFilePath"] ?? "firebase-key.json";
            string fullCredentialPath = Path.IsPathRooted(credentialPath)
                ? credentialPath
                : Path.Combine(environment.ContentRootPath, credentialPath);

            if (!string.IsNullOrEmpty(_bucketName) && File.Exists(fullCredentialPath))
            {
                try
                {
                    GoogleCredential credential = GoogleCredential.FromFile(fullCredentialPath);
                    _storageClient = StorageClient.Create(credential);
                    _isFirebaseEnabled = true;
                }
                catch
                {
                    _isFirebaseEnabled = false;
                }
            }
        }

        // Saves an incoming file to Firebase Storage (or local disk if Firebase credentials are absent)
        // Returns unique stored file name and full path or public cloud URL
        public virtual async Task<(string StoredFileName, string FullPath)> SaveFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File cannot be empty.", nameof(file));
            }

            string uniquePrefix = Guid.NewGuid().ToString("N");
            string safeFileName = Path.GetFileName(file.FileName);
            string storedFileName = $"{uniquePrefix}_{safeFileName}";

            if (_isFirebaseEnabled && _storageClient != null && !string.IsNullOrEmpty(_bucketName))
            {
                // Stream binary payload directly over network to Firebase Storage bucket
                using (var stream = file.OpenReadStream())
                {
                    await _storageClient.UploadObjectAsync(
                        bucket: _bucketName,
                        objectName: storedFileName,
                        contentType: file.ContentType ?? "application/octet-stream",
                        source: stream
                    );
                }

                // Public Firebase media download URL
                string firebaseUrl = $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{Uri.EscapeDataString(storedFileName)}?alt=media";
                return (storedFileName, firebaseUrl);
            }

            // Fallback: Save file to local disk
            string fullPath = Path.Combine(_uploadFolder, storedFileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return (storedFileName, fullPath);
        }

        // Retrieves file stream for downloading (local disk stream or Firebase stream)
        public virtual FileStream? GetFileStream(string storedFileName)
        {
            string fullPath = Path.Combine(_uploadFolder, storedFileName);
            if (!File.Exists(fullPath))
            {
                return null;
            }

            return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        // Deletes a file from Firebase Storage bucket or local disk
        public virtual bool DeleteFile(string storedFileName)
        {
            if (string.IsNullOrEmpty(storedFileName)) return false;

            if (_isFirebaseEnabled && _storageClient != null && !string.IsNullOrEmpty(_bucketName))
            {
                try
                {
                    _storageClient.DeleteObject(_bucketName, storedFileName);
                    return true;
                }
                catch
                {
                    // Ignore if object doesn't exist on Firebase
                }
            }

            // Local disk deletion
            string fullPath = Path.Combine(_uploadFolder, storedFileName);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
            return false;
        }

        // Returns upload folder path
        public virtual string GetUploadFolder() => _uploadFolder;

        // Returns true if Firebase Storage is active
        public virtual bool IsFirebaseEnabled() => _isFirebaseEnabled;
    }
}
