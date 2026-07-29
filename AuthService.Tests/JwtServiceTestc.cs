using AuthService.API.Services;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace AuthService.Test
{
    public class JwtServiceTests
    {
        private readonly JwtService _jwtService;

        public JwtServiceTests()
        {
            // Create test JWT configuration
            var settings = new Dictionary<string, string?>
            {
                ["Jwt:Key"] =
                    "ThisIsASecretKeyForTestingJwt123456789",

                ["Jwt:Issuer"] =
                    "AuthService.Test",

                ["Jwt:Audience"] =
                    "AuthService.Test",

                ["Jwt:ExprireMinutes"] =
                    "60"
            };

            IConfiguration configuration =
                new ConfigurationBuilder()
                    .AddInMemoryCollection(settings)
                    .Build();

            _jwtService =
                new JwtService(configuration);
        }


        [Fact]
        public void GenerateToken_ReturnsToken()
        {
            // Arrange
            string userId = "123";
            string email = "test@example.com";

            // Act
            string token =
                _jwtService.GenerateToken(
                    userId,
                    email
                );

            // Assert
            Assert.False(
                string.IsNullOrEmpty(token)
            );
        }


        [Fact]
        public void GenerateToken_ReturnsValidJwt()
        {
            // Arrange
            string userId = "123";
            string email = "test@example.com";

            // Act
            string token =
                _jwtService.GenerateToken(
                    userId,
                    email
                );

            // Assert
            var handler =
                new JwtSecurityTokenHandler();

            Assert.True(
                handler.CanReadToken(token)
            );
        }
    }
}