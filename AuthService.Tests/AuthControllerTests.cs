using AuthService.API.Controllers;
using AuthService.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.Tests
{
    public class AuthControllerTests
    {
        [Fact]
        public void Me_ReturnsEmailFromClaims()
        {
            var controller = new AuthController(new Auth(null!, null!, null!, null!));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Email, "user@example.com")
                    }, "TestAuth"))
                }
            };

            var result = controller.Me();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<Dictionary<string, string>>(okResult.Value);

            Assert.Equal("user@example.com", payload["email"]);
        }
    }
}
