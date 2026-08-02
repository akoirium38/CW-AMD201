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

            // Option A: Upload directly to Firebase Cloud Storage bucket if credentials active
            if (_isFirebaseEnabled && _storageClient != null && !string.IsNullOrEmpty(_bucketName))
            {
                // Try primary bucket name (e.g., "amd201-cb545.firebasestorage.app") and fallback bucket name ("amd201-cb545.appspot.com")
                string[] bucketCandidates = new string[] { _bucketName, "amd201-cb545.appspot.com", "amd201-cb545" };

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
                    catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // If specific bucket format was not found, continue trying next candidate bucket name
                        continue;
                    }
                    catch (Exception)
                    {
                        // On other network/cloud errors, break loop to fall back to local disk storage
                        break;
                    }
                }
            }

            // Option B: Fallback to local server disk storage if Firebase Storage bucket unavailable
            string fullPath = Path.Combine(_uploadFolder, storedFileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return (storedFileName, fullPath);
        }

        /// <summary>
        /// Retrieves a FileStream for reading file content from local server storage.
        /// </summary>
        /// <param name="storedFileName">Unique file name stored on disk</param>
        /// <returns>FileStream or null if file not found</returns>
        public virtual FileStream? GetFileStream(string storedFileName)
        {
            string fullPath = Path.Combine(_uploadFolder, storedFileName);
            if (!File.Exists(fullPath))
            {
                return null;
            }

            return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
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
