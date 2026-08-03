using System;

namespace FileService.API.DTOs
{
    /// <summary>
    /// DTO containing security and access settings updates for an existing file.
    /// Used by PUT /api/files/{id}/access
    /// 
    /// 🔗 Architecture Links:
    /// - API Endpoint: Used as [FromBody] parameter in PUT /api/files/{id}/access (FilesController.cs)
    /// - Service Layer: Processed inside UpdateFileAccessAsync (FileService.cs)
    /// </summary>
    public class UpdateFileAccessDto
    {
        /// <summary>
        /// New protection password.
        /// Pass a new password string (e.g., "newsecret123") to set or change password protection.
        /// Pass empty string "" to remove password protection entirely.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// New expiration date string (e.g., "2026-12-31" or "2026-12-31T23:59:59Z").
        /// Pass empty string "" to remove expiration date.
        /// </summary>
        public string? ExpiryDate { get; set; }

        /// <summary>
        /// New maximum number of times the file can be downloaded (e.g., 5, 10).
        /// Pass null to allow unlimited downloads.
        /// </summary>
        public int? MaxDownloads { get; set; }
    }
}
