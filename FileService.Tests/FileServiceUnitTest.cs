using FileService.API.Data;
using FileService.API.DTOs;
using FileService.API.Models;
using FileService.API.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FileService.Tests
{
    public class FileServiceUnitTest
    {
        private readonly Mock<FileDbContext> _mockDbContext;
        private readonly Mock<IMongoCollection<FileRecord>> _mockCollection;
        private readonly Mock<StorageService> _mockStorageService;
        private readonly Mock<ThumbnailService> _mockThumbnailService;
        private readonly Mock<UploadLimitService> _mockUploadLimitService;
        private readonly FileService.API.Services.FileService _fileService;

        public FileServiceUnitTest()
        {
            _mockCollection = new Mock<IMongoCollection<FileRecord>>();
            _mockDbContext = new Mock<FileDbContext>();
            _mockDbContext.Setup(db => db.Files).Returns(_mockCollection.Object);

            _mockStorageService = new Mock<StorageService>();
            _mockThumbnailService = new Mock<ThumbnailService>();
            _mockUploadLimitService = new Mock<UploadLimitService>();

            _fileService = new FileService.API.Services.FileService(
                _mockDbContext.Object,
                _mockStorageService.Object,
                _mockThumbnailService.Object,
                _mockUploadLimitService.Object
            );
        }

        private static IFormFile CreateMockFormFile(string fileName, string contentType, long length = 1024)
        {
            var fileMock = new Mock<IFormFile>();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("dummy content"));
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.ContentType).Returns(contentType);
            fileMock.Setup(f => f.Length).Returns(length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
            return fileMock.Object;
        }

        private static string HashPassword(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        [Fact]
        public async Task UploadFileAsync_ValidFile_ReturnsMappedResponseDto()
        {
            // Arrange
            int userId = 10;
            var formFile = CreateMockFormFile("document.pdf", "application/pdf");
            var dto = new UploadFileRequestDto
            {
                File = formFile,
                Password = "secretPassword",
                ExpiryDate = "2026-12-31"
            };

            _mockUploadLimitService
                .Setup(s => s.ValidateSingleFileSize(It.IsAny<long>()))
                .Returns((true, string.Empty));

            _mockUploadLimitService
                .Setup(s => s.CheckUserQuotaAsync(userId, It.IsAny<long>()))
                .ReturnsAsync((true, string.Empty));

            _mockStorageService
                .Setup(s => s.SaveFileAsync(formFile))
                .ReturnsAsync(("guid_document.pdf", "/Uploads/guid_document.pdf"));

            _mockThumbnailService
                .Setup(s => s.GenerateThumbnailAsync(formFile, "guid_document.pdf"))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _fileService.UploadFileAsync(userId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("document.pdf", result.FileName);
            Assert.Equal("application/pdf", result.ContentType);
            Assert.True(result.HasPassword);
            Assert.NotNull(result.FileId);
            _mockCollection.Verify(c => c.InsertOneAsync(It.IsAny<FileRecord>(), null, default), Times.Once);
        }

        [Fact]
        public async Task UploadFileAsync_SingleFileOversized_ThrowsInvalidOperationException()
        {
            // Arrange
            int userId = 1;
            var formFile = CreateMockFormFile("large.zip", "application/zip", 60 * 1024 * 1024);
            var dto = new UploadFileRequestDto { File = formFile };

            _mockUploadLimitService
                .Setup(s => s.ValidateSingleFileSize(It.IsAny<long>()))
                .Returns((false, "File size exceeds limit."));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _fileService.UploadFileAsync(userId, dto));

            Assert.Equal("File size exceeds limit.", ex.Message);
        }

        [Fact]
        public async Task UploadFileAsync_QuotaExceeded_ThrowsInvalidOperationException()
        {
            // Arrange
            int userId = 1;
            var formFile = CreateMockFormFile("data.bin", "application/octet-stream");
            var dto = new UploadFileRequestDto { File = formFile };

            _mockUploadLimitService
                .Setup(s => s.ValidateSingleFileSize(It.IsAny<long>()))
                .Returns((true, string.Empty));

            _mockUploadLimitService
                .Setup(s => s.CheckUserQuotaAsync(userId, It.IsAny<long>()))
                .ReturnsAsync((false, "Quota limit exceeded."));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _fileService.UploadFileAsync(userId, dto));

            Assert.Equal("Quota limit exceeded.", ex.Message);
        }

        [Fact]
        public void MapToDto_CorrectlyMapsAllFields()
        {
            // Arrange
            var record = new FileRecord
            {
                Id = "64a1f2b3c4d5e6f7a8b9c0d1",
                UserId = 5,
                FileName = "photo.png",
                StoredFileName = "guid_photo.png",
                ContentType = "image/png",
                FileSizeBytes = 2048,
                UploadDate = new DateTime(2026, 7, 29),
                PasswordHash = HashPassword("mypassword"),
                ExpiryDate = new DateTime(2026, 8, 1),
                DownloadLimit = 5,
                DownloadCount = 2,
                ThumbnailPath = "thumb_guid_photo.png"
            };

            // Act
            var dto = API.Services.FileService.MapToDto(record);

            // Assert
            Assert.Equal("64a1f2b3c4d5e6f7a8b9c0d1", dto.FileId);
            Assert.Equal("photo.png", dto.FileName);
            Assert.Equal(2048, dto.Size);
            Assert.True(dto.HasPassword);
            Assert.Equal(5, dto.DownloadLimit);
            Assert.Equal(2, dto.DownloadCount);
            Assert.Equal("/api/files/download/64a1f2b3c4d5e6f7a8b9c0d1", dto.DownloadUrl);
            Assert.Equal("/api/files/64a1f2b3c4d5e6f7a8b9c0d1/thumbnail", dto.ThumbnailUrl);
        }

        [Fact]
        public async Task UploadFileAsync_ImageFile_GeneratesThumbnail()
        {
            // Arrange
            int userId = 20;
            var formFile = CreateMockFormFile("avatar.jpg", "image/jpeg");
            var dto = new UploadFileRequestDto { File = formFile };

            _mockUploadLimitService
                .Setup(s => s.ValidateSingleFileSize(It.IsAny<long>()))
                .Returns((true, string.Empty));

            _mockUploadLimitService
                .Setup(s => s.CheckUserQuotaAsync(userId, It.IsAny<long>()))
                .ReturnsAsync((true, string.Empty));

            _mockStorageService
                .Setup(s => s.SaveFileAsync(formFile))
                .ReturnsAsync(("guid_avatar.jpg", "/Uploads/guid_avatar.jpg"));

            _mockThumbnailService
                .Setup(s => s.GenerateThumbnailAsync(formFile, "guid_avatar.jpg"))
                .ReturnsAsync("thumb_guid_avatar.jpg");

            // Act
            var result = await _fileService.UploadFileAsync(userId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("/thumbnail", result.ThumbnailUrl);
            _mockThumbnailService.Verify(t => t.GenerateThumbnailAsync(formFile, "guid_avatar.jpg"), Times.Once);
        }

        [Fact]
        public async Task UploadFileAsync_WithPassword123456_HashesPasswordAndVerifies()
        {
            // Arrange
            int userId = 101;
            var formFile = CreateMockFormFile("confidential.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            var dto = new UploadFileRequestDto
            {
                File = formFile,
                Password = "123456"
            };

            _mockUploadLimitService
                .Setup(s => s.ValidateSingleFileSize(It.IsAny<long>()))
                .Returns((true, string.Empty));

            _mockUploadLimitService
                .Setup(s => s.CheckUserQuotaAsync(userId, It.IsAny<long>()))
                .ReturnsAsync((true, string.Empty));

            _mockStorageService
                .Setup(s => s.SaveFileAsync(formFile))
                .ReturnsAsync(("guid_confidential.docx", "https://firebasestorage.googleapis.com/v0/b/amd201-cb545.firebasestorage.app/o/guid_confidential.docx?alt=media"));

            // Act
            var result = await _fileService.UploadFileAsync(userId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("confidential.docx", result.FileName);
            Assert.True(result.HasPassword);
            Assert.NotNull(result.FileId);
        }

        [Fact]
        public void UpdateFileRequestDto_CanSetAndGetProperties()
        {
            // Arrange & Act
            var dto = new UpdateFileRequestDto
            {
                FileName = "New_Name.pdf",
                Password = "newpassword123",
                ExpiryDate = "2026-12-31",
                DownloadLimit = 10
            };

            // Assert
            Assert.Equal("New_Name.pdf", dto.FileName);
            Assert.Equal("newpassword123", dto.Password);
            Assert.Equal("2026-12-31", dto.ExpiryDate);
            Assert.Equal(10, dto.DownloadLimit);
            Assert.Equal(10, dto.MaxDownloads);
        }
    }
}