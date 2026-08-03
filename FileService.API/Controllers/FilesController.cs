using FileService.API.DTOs;
using FileService.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FileService.API.Controllers
{
    /// <summary>
    /// FilesController exposes RESTful HTTP endpoints for file uploading, listing, downloading,
    /// password protection verification, quota tracking, thumbnail streaming, and deletion.
    /// 
    /// 🔗 Architecture Links:
    /// - Microservice Gateway: Requests routed via Ocelot API Gateway (http://localhost:7000/api/files/* -> http://localhost:5201/api/files/*)
    /// - Authentication: Secured using [Authorize] attribute which parses JWT Bearer tokens issued by AuthService.API
    /// - Frontend Integration: Matched directly with TypeScript Axios calls in fe/src/services/fileService.ts
    /// - Database: Interacts with MongoDB Atlas via FileService.cs
    /// - Cloud Storage: Integrates with Firebase Storage via StorageService.cs
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly Services.FileService _fileService;
        private readonly UploadLimitService _uploadLimitService;

        /// <summary>
        /// Constructor injected by ASP.NET Core Dependency Injection container.
        /// </summary>
        public FilesController(Services.FileService fileService, UploadLimitService uploadLimitService)
        {
            _fileService = fileService;
            _uploadLimitService = uploadLimitService;
        }

        /// <summary>
        /// Private helper method to extract the logged-in User ID from JWT Token claims.
        /// Extracts ClaimTypes.NameIdentifier embedded in JWT header by AuthService.API.
        /// Handles integer IDs as well as non-integer MongoDB string IDs safely.
        /// </summary>
        /// <returns>Integer User ID</returns>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            // Try parsing integer ID directly
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            
            // Fallback for string ObjectIds (e.g. MongoDB ObjectIds) by hashing string to positive int
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                return Math.Abs(userIdClaim.GetHashCode());
            }
            
            throw new UnauthorizedAccessException("User session invalid or missing NameIdentifier claim.");
        }

        /// <summary>
        /// POST: /api/files/upload
        /// Handles multipart/form-data file uploads from frontend FileHub dashboard or Swagger UI.
        /// Uploads physical binary to Firebase Storage and metadata to MongoDB Atlas.
        /// </summary>
        [HttpPost("upload")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileRequestDto request)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new { message = "Please select a valid non-empty file to upload." });
            }

            try
            {
                int userId = GetCurrentUserId();
                var result = await _fileService.UploadFileAsync(userId, request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                // Single file size limit or storage quota exceeded
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while uploading file: " + ex.Message });
            }
        }

        /// <summary>
        /// GET: /api/files
        /// Retrieves list of file records uploaded by the currently authenticated user.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserFiles()
        {
            try
            {
                int userId = GetCurrentUserId();
                var files = await _fileService.GetUserFilesAsync(userId);
                return Ok(files);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: /api/files/my-files
        /// Alias endpoint matching frontend React call fileService.fetchMyFiles().
        /// Returns all file records for the logged-in user sorted newest-first.
        /// </summary>
        [HttpGet("my-files")]
        [Authorize]
        public async Task<IActionResult> GetMyFiles()
        {
            try
            {
                int userId = GetCurrentUserId();
                var files = await _fileService.GetUserFilesAsync(userId);
                return Ok(files);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: /api/files/storage-quota
        /// Calculates storage quota usage (bytes used vs total allowed limit) for user dashboard progress bar.
        /// </summary>
        [HttpGet("storage-quota")]
        [Authorize]
        public async Task<IActionResult> GetStorageQuota()
        {
            try
            {
                int userId = GetCurrentUserId();
                var quota = await _uploadLimitService.GetUserStorageQuotaAsync(userId);
                return Ok(quota);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: /api/files/{id}
        /// Fetches details for a single file record by MongoDB ObjectId string (e.g., "64a1f2b3c4d5e6f7a8b9c0d1").
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetFileById(string id)
        {
            var file = await _fileService.GetFileByIdAsync(id);
            if (file == null)
            {
                return NotFound(new { message = "File record not found." });
            }

            return Ok(file);
        }

        /// <summary>
        /// POST: /api/files/{id}/verify-password
        /// Validates user-entered password against SHA256 password hash stored in MongoDB Atlas before granting download access.
        /// Allowed for anonymous users so shared links can be password-protected.
        /// </summary>
        [HttpPost("{id}/verify-password")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyPassword(string id, [FromBody] VerifyPasswordRequestDto dto)
        {
            bool isValid = await _fileService.VerifyPasswordAsync(id, dto?.Password ?? string.Empty);
            if (!isValid)
            {
                return BadRequest(new { isSuccess = false, message = "Incorrect password." });
            }

            return Ok(new { isSuccess = true, message = "Password verified successfully." });
        }

        /// <summary>
        /// GET: /api/files/download/{id}
        /// Download route for fetching binary file streams with optional password check.
        /// </summary>
        [HttpGet("download/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadFile(string id, [FromQuery] string? password)
        {
            var (stream, contentType, fileName, errorMessage) = await _fileService.PrepareDownloadAsync(id, password);
            if (errorMessage != null)
            {
                return BadRequest(new { message = errorMessage });
            }

            if (stream == null)
            {
                return NotFound(new { message = "File binary content missing." });
            }

            return File(stream, contentType, fileName);
        }



        /// <summary>
        /// GET: /api/files/{id}/thumbnail
        /// Streams PNG image thumbnail generated for uploaded image files (e.g. JPG, PNG, WEBP).
        /// </summary>
        [HttpGet("{id}/thumbnail")]
        [AllowAnonymous]
        public async Task<IActionResult> GetThumbnail(string id)
        {
            var stream = await _fileService.GetThumbnailStreamAsync(id);
            if (stream == null)
            {
                return NotFound(new { message = "Thumbnail not available." });
            }

            return File(stream, "image/png");
        }

        /// <summary>
        /// PUT: /api/files/{id}
        /// Updates existing file metadata (FileName, Password, ExpiryDate) in MongoDB Atlas.
        /// Requires JWT Authentication [Authorize] and verifies user ownership.
        /// </summary>
        /// <param name="id">Target file ID</param>
        /// <param name="request">Update DTO containing modified properties</param>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateFile(string id, [FromBody] UpdateFileRequestDto request)
        {
            try
            {
                // Extract current user ID from JWT token claims
                int userId = GetCurrentUserId();

                // Call FileService to apply updates in MongoDB Atlas
                var updatedFile = await _fileService.UpdateFileMetadataAsync(id, userId, request);

                if (updatedFile == null)
                {
                    return NotFound(new { message = "File not found or you do not have permission to update it." });
                }

                return Ok(new { isSuccess = true, message = "File metadata updated successfully.", data = updatedFile });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating file: " + ex.Message });
            }
        }

        /// <summary>
        /// DELETE: /api/files/{id}
        /// Deletes a file record from MongoDB Atlas and purges the physical file from Firebase Storage bucket.
        /// Only allowed if the logged-in user matches the owner UserId.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteFile(string id)
        {
            try
            {
                int userId = GetCurrentUserId();
                bool deleted = await _fileService.DeleteFileAsync(id, userId);
                if (!deleted)
                {
                    return NotFound(new { message = "File not found or you do not have permission to delete it." });
                }

                return Ok(new { isSuccess = true, message = "File deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
