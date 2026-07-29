using FileService.API.DTOs;
using FileService.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FileService.API.Controllers
{
    // API Controller exposing endpoints for file upload, download, password validation, deletion, and quota
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly Services.FileService _fileService;
        private readonly UploadLimitService _uploadLimitService;

        public FilesController(Services.FileService fileService, UploadLimitService uploadLimitService)
        {
            _fileService = fileService;
            _uploadLimitService = uploadLimitService;
        }

        // Helper method to retrieve current logged-in User ID from JWT claim
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User session invalid or missing NameIdentifier claim.");
        }

        // POST: /api/files/upload
        // Uploads a new file for the authenticated user
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
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while uploading file: " + ex.Message });
            }
        }

        // GET: /api/files
        // Retrieves list of files uploaded by authenticated user
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

        // GET: /api/files/my-files
        // Alias route matching the frontend fileService.fetchMyFiles() call
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

        // GET: /api/files/storage-quota
        // Retrieves storage quota usage for authenticated user
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

        // GET: /api/files/{id}
        // Retrieves details for a specific file
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetFileById(int id)
        {
            var file = await _fileService.GetFileByIdAsync(id);
            if (file == null)
            {
                return NotFound(new { message = "File record not found." });
            }

            return Ok(file);
        }

        // POST: /api/files/{id}/verify-password
        // Validates password for protected file download
        [HttpPost("{id:int}/verify-password")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyPassword(int id, [FromBody] VerifyPasswordRequestDto dto)
        {
            bool isValid = await _fileService.VerifyPasswordAsync(id, dto?.Password ?? string.Empty);
            if (!isValid)
            {
                return BadRequest(new { isSuccess = false, message = "Incorrect password." });
            }

            return Ok(new { isSuccess = true, message = "Password verified successfully." });
        }

        // GET: /api/files/{id}/download
        // Original download route (kept for backward compatibility)
        [HttpGet("{id:int}/download")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadFile(int id, [FromQuery] string? password)
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

        // GET: /api/files/download/{id}
        // Frontend-compatible download route matching fileService.downloadFile(fileId)
        [HttpGet("download/{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadFileAlt(int id, [FromQuery] string? password)
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

        // GET: /api/files/{id}/thumbnail
        // Streams thumbnail image for a file
        [HttpGet("{id:int}/thumbnail")]
        [AllowAnonymous]
        public async Task<IActionResult> GetThumbnail(int id)
        {
            var stream = await _fileService.GetThumbnailStreamAsync(id);
            if (stream == null)
            {
                return NotFound(new { message = "Thumbnail not available." });
            }

            return File(stream, "image/png");
        }

        // DELETE: /api/files/{id}
        // Deletes a file record and disk content owned by authenticated user
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteFile(int id)
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
