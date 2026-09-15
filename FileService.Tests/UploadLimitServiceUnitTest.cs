using FileService.API.Services;
using Xunit;

namespace FileService.Tests
{
    public class UploadLimitServiceUnitTest
    {
        private readonly UploadLimitService _uploadLimitService = new();

        [Fact]
        public void ValidateSingleFileSize_ZeroBytes_ReturnsInvalid()
        {
            var (isValid, message) = _uploadLimitService.ValidateSingleFileSize(0);

            Assert.False(isValid);
            Assert.Equal("File cannot be empty.", message);
        }

        [Fact]
        public void ValidateSingleFileSize_NegativeBytes_ReturnsInvalid()
        {
            var (isValid, message) = _uploadLimitService.ValidateSingleFileSize(-1);

            Assert.False(isValid);
            Assert.Equal("File cannot be empty.", message);
        }

        [Fact]
        public void ValidateSingleFileSize_ExceedsMaxSingleFileSize_ReturnsInvalid()
        {
            long oversized = UploadLimitService.MaxSingleFileSizeBytes + 1;

            var (isValid, message) = _uploadLimitService.ValidateSingleFileSize(oversized);

            Assert.False(isValid);
            Assert.Contains("50 MB", message);
        }

        [Fact]
        public void ValidateSingleFileSize_ExactlyAtMaxSingleFileSize_ReturnsValid()
        {
            var (isValid, message) = _uploadLimitService.ValidateSingleFileSize(UploadLimitService.MaxSingleFileSizeBytes);

            Assert.True(isValid);
            Assert.Equal(string.Empty, message);
        }

        [Fact]
        public void ValidateSingleFileSize_WithinLimit_ReturnsValid()
        {
            var (isValid, message) = _uploadLimitService.ValidateSingleFileSize(1024);

            Assert.True(isValid);
            Assert.Equal(string.Empty, message);
        }
    }
}
