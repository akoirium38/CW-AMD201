using System;

namespace FileService.API.DTOs
{
    /// <summary>
    /// UpdateFileRequestDto contains the metadata properties a user is allowed to update
    /// for an existing uploaded file in FileHub.
    /// 
    /// 🔗 Architecture Links:
    /// - API Endpoint: Used as [FromBody] parameter in PUT /api/files/{id} (FilesController.cs)
    /// - Service Layer: Processed inside UpdateFileMetadataAsync (FileService.cs)
    /// </summary>
    public class UpdateFileRequestDto
    {
        /// <summary>
        /// New user-friendly display name for the file (e.g., "Updated_Report.pdf").
        /// Optional: If left null or empty, the original file name is kept.
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// New protection password for the file (e.g., "newpassword123").
        /// Pass null to keep current password, or empty string "" to remove password protection.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// New expiration date string (e.g., "2026-12-31T23:59:59Z" or "2026-12-31").
        /// Pass null to keep current expiry date, or empty string "" to clear expiration.
        /// </summary>
        public string? ExpiryDate { get; set; }
    }
}
