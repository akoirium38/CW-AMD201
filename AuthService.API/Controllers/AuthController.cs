using AuthService.API.Controllers;
using AuthService.API.DTOs;
using AuthService.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly Auth _authService;

        public AuthController(
            Auth authService)
        {
            _authService = authService;
        }

        [HttpPost("request-otp")]
        public async Task<IActionResult> RequestOtp(string email)
        {
            await _authService.RequestOtpAsync(email);

            return Ok(new
            {
                success = true,
                message = "OTP has been sentl."
            });
        }
    }
}
