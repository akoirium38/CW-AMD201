using AuthService.API.Controllers;
using AuthService.API.DTOs;
using AuthService.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        public async Task<IActionResult> RequestOtp([FromBody] RequestOtpDto request)
        {
            await _authService.RequestOtpAsync(request.Email);

            return Ok(new
            {
                success = true,
                message = "OTP has been sent."
            });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> CheckOtp([FromBody] RequestVerifyOtpDto request)
        {
            var token = await _authService.VerifyOtp(request.Code, request.Email);

            if (token == null)
            {
                return BadRequest();
            }

            return Ok(new
            {
                token = token
            });
        }

        [Authorize]
        [HttpPost("Check_Authorize")]
        public async Task<IActionResult> CheckAuthorize()
        {
            return Ok("Authorized");
        }

        [Authorize]
        [HttpGet("my-id")]
        public IActionResult GetMyId()
        {
            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            return Ok(new
            {
                UserId = userId
            });
        }

    }
}
