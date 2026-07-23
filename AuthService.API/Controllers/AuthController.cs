using AuthService.API.Controllers;
using AuthService.API.DTOs;
using AuthService.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto;

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
                message = "OTP has been sent."
            });
        }

        [HttpPost("Check_Otp")]
        public async Task<IActionResult> CheckOtp(string OtpCode, string gmail)
        {
            var token = await _authService.VerifyOtp(OtpCode, gmail);

            if (token == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid or expired OTP."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Login successful",
                token = token
            });
        }

        [Authorize]
        [HttpPost("Check_Authorize")]
        public async Task<IActionResult> CheckAuthorize()
        {
            return Ok("Authorized");
        }

    }
}
