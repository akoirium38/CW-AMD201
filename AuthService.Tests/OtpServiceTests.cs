using AuthService.API.Services;
using Xunit;

namespace AuthService.Test
{
    public class OtpServiceTests
    {
        private readonly OtpService _otpService;

        public OtpServiceTests()
        {
            // OtpService requires AuthDbContext,
            // so we pass null because these tests
            // only test CreateOtpCode().
            _otpService = new OtpService(null!);
        }

        [Fact]
        public void CreateOtpCode_DefaultLength_ReturnsSixCharacters()
        {
            // Act
            string result = _otpService.CreateOtpCode();

            // Assert
            Assert.Equal(6, result.Length);
        }

        [Fact]
        public void CreateOtpCode_CustomLength_ReturnsCorrectLength()
        {
            // Arrange
            int length = 10;

            // Act
            string result = _otpService.CreateOtpCode(length);

            // Assert
            Assert.Equal(length, result.Length);
        }

        [Fact]
        public void CreateOtpCode_ContainsOnlyAllowedCharacters()
        {
            // Arrange
            const string allowedCharacters =
                "qwertyuiopasdfghjklzxcvbnm" +
                "QWERTYUIOPASDFGHJKLZXCVBNM" +
                "1234567890";

            // Act
            string result = _otpService.CreateOtpCode();

            // Assert
            Assert.All(
                result,
                character =>
                    Assert.Contains(
                        character,
                        allowedCharacters
                    )
            );
        }

        [Fact]
        public void CreateOtpCode_ReturnsDifferentCodes()
        {
            // Act
            string firstCode =
                _otpService.CreateOtpCode();

            string secondCode =
                _otpService.CreateOtpCode();

            // Assert
            Assert.NotEqual(
                firstCode,
                secondCode
            );
        }
    }
}
