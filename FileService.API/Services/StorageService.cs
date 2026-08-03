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
    /// <summary>
    /// StorageService handles physical file binary storage, retrieval, and deletion.
    /// Integrated with Google Cloud Storage V1 SDK to store files in Firebase Storage cloud buckets,
    /// with an automatic local disk fallback for offline development.
    /// 
    /// 🔗 Architecture Links:
    /// - Configuration: Reads "Firebase:BucketName" and "Firebase:CredentialFilePath" from appsettings.json
    /// - Service Consumer: Called directly by FileService.cs during file upload and deletion workflows
    /// - External Cloud: Connects to Firebase Cloud Storage (amd201-cb545.firebasestorage.app)
    /// </summary>
    public class StorageService
    {
        // Google Cloud Storage SDK client for interacting with Firebase Cloud Storage
        private readonly StorageClient? _storageClient;
        
        // Target Firebase bucket name (e.g., "amd201-cb545.firebasestorage.app")
        private readonly string? _bucketName;
        
        // Path to local "Uploads" folder on server disk (used for fallback or thumbnail storage)
        private readonly string _uploadFolder = string.Empty;
        
        // Flag indicating whether Firebase credentials were valid and cloud connection is active
        private readonly bool _isFirebaseEnabled = false;

        /// <summary>
        /// Default parameterless constructor required by Moq for unit testing (FileServiceUnitTest.cs).
        /// </summary>
        public StorageService() { }

        /// <summary>
        /// Dependency Injection Constructor. Initializes local storage folder and authenticates with Firebase.
        /// </summary>
        /// <param name="environment">Provides current content root directory path</param>
        /// <param name="configuration">Accesses appsettings.json settings</param>
        public StorageService(IWebHostEnvironment environment, IConfiguration configuration)
        {
            // 1. Setup local disk fallback folder ("{ContentRoot}/Uploads")
            _uploadFolder = Path.Combine(environment.ContentRootPath, "Uploads");
            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }

            // 2. Read Firebase bucket name and JSON Service Account credentials key file path from appsettings.json
            _bucketName = configuration["Firebase:BucketName"]!;

            string credentialPath =
                Environment.GetEnvironmentVariable("CREDENTIALS") // for deployed on render
                ?? configuration["Firebase:CredentialFilePath"] // for testing localy
                ?? throw new InvalidOperationException("Firebase credential path is not configured.");

            // Resolve relative credential file path to absolute root path
            string fullCredentialPath = Path.IsPathRooted(credentialPath)
                ? credentialPath
                : Path.Combine(environment.ContentRootPath, credentialPath);

            // 3. If credentials file exists, authenticate using Google OAuth2 Service Account
            if (!string.IsNullOrEmpty(_bucketName) && File.Exists(fullCredentialPath))
            {
                try
                {
                    // Load service account private key & credentials from JSON file
                    GoogleCredential credential = GoogleCredential.FromFile(fullCredentialPath);
                    
                    // Create authorized StorageClient for Google Cloud / Firebase API requests
                    _storageClient = StorageClient.Create(credential);
                    _isFirebaseEnabled = true;
                }
                catch (Exception)
                {
                    // If credentials invalid or network fails, fallback gracefully to local disk storage
                    _isFirebaseEnabled = false;
                }
            }
        }

        /// <summary>
        /// Saves an uploaded IFormFile to Firebase Storage bucket or local disk.
        /// Generates a unique stored file name (UUID prefix) to prevent naming collisions.
        /// </summary>
        /// <param name="file">Form file submitted from frontend / Swagger</param>
        /// <returns>Tuple containing (uniqueStoredFileName, fullPathOrCloudUrl)</returns>
        public virtual async Task<(string StoredFileName, string FullPath)> SaveFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File cannot be empty.", nameof(file));
            }

            // Generate a 32-character hexadecimal GUID prefix to guarantee unique stored file names
            string uniquePrefix = Guid.NewGuid().ToString("N");
            string safeFileName = Path.GetFileName(file.FileName);
            string storedFileName = $"{uniquePrefix}_{safeFileName}";

            // Save to local server disk storage to guarantee all file types can be streamed for download
            string fullPath = Path.Combine(_uploadFolder, storedFileName);
            using (var localStream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(localStream);
            }

            // Option A: Upload to Firebase Cloud Storage bucket if credentials active
            if (_isFirebaseEnabled && _storageClient != null && !string.IsNullOrEmpty(_bucketName))
            {
                string[] bucketCandidates = new string[] { _bucketName, "amd201-cb545.firebasestorage.app", "amd201-cb545.appspot.com", "amd201-cb545" };

                foreach (var targetBucket in bucketCandidates)
                {
                    try
                    {
                        using (var stream = file.OpenReadStream())
                        {
                            await _storageClient.UploadObjectAsync(
                                bucket: targetBucket,
                                objectName: storedFileName,
                                contentType: file.ContentType ?? "application/octet-stream",
                                source: stream
                            );
                        }

                        // Construct public Firebase Storage media download URL
                        string firebaseUrl = $"https://firebasestorage.googleapis.com/v0/b/{targetBucket}/o/{Uri.EscapeDataString(storedFileName)}?alt=media";
                        return (storedFileName, firebaseUrl);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            return (storedFileName, fullPath);
        }

        /// <summary>
        /// Retrieves a Stream for reading file content from local server storage or Firebase Cloud Storage.
        /// </summary>
        /// <param name="storedFileName">Unique stored file name</param>
        /// <returns>Stream or null if file not found</returns>
        public virtual async Task<Stream?> GetFileStreamAsync(string storedFileName)
        {
            // 1. Check local server disk storage first
            string fullPath = Path.Combine(_uploadFolder, storedFileName);
            if (File.Exists(fullPath))
            {
                return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            // 2. Fallback to fetch from Firebase Cloud Storage bucket if not on local disk
            if (_isFirebaseEnabled && _storageClient != null && !string.IsNullOrEmpty(_bucketName))
            {
                string[] bucketCandidates = new string[] { _bucketName, "amd201-cb545.firebasestorage.app", "amd201-cb545.appspot.com", "amd201-cb545" };
                foreach (var targetBucket in bucketCandidates)
                {
                    try
                    {
                        var memoryStream = new MemoryStream();
                        await _storageClient.DownloadObjectAsync(targetBucket, storedFileName, memoryStream);
                        memoryStream.Position = 0;
                        return memoryStream;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Synchronous retrieval fallback for legacy calls.
        /// </summary>
        public virtual FileStream? GetFileStream(string storedFileName)
        {
            string fullPath = Path.Combine(_uploadFolder, storedFileName);
            if (File.Exists(fullPath))
            {
                return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            return null;
        }

        /// <summary>
        /// Deletes a file from Firebase Cloud Storage bucket or local server disk.
        /// </summary>
        /// <param name="storedFileName">Unique stored object name in bucket or disk</param>
        /// <returns>True if successfully deleted</returns>
        public virtual bool DeleteFile(string storedFileName)
        {
            if (string.IsNullOrEmpty(storedFileName)) return false;

            // Delete from Firebase Storage if cloud mode enabled
            if (_isFirebaseEnabled && _storageClient != null && !string.IsNullOrEmpty(_bucketName))
            {
                try
                {
                    _storageClient.DeleteObject(_bucketName, storedFileName);
                    return true;
                }
                catch (Exception)
                {
                    // Ignore if object already missing from Firebase bucket
                }
            }

            // Delete from local server disk
            string fullPath = Path.Combine(_uploadFolder, storedFileName);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns local upload folder path.
        /// </summary>
        public virtual string GetUploadFolder() => _uploadFolder;

        /// <summary>
        /// Checks whether Firebase Storage cloud integration is active.
        /// </summary>
        public virtual bool IsFirebaseEnabled() => _isFirebaseEnabled;
    }
}
