using AuthService.API.Controllers;
using AuthService.API.DTOs;
using AuthService.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Bcpg.OpenPgp;
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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Gmail))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Gmail is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Password is required."
                });
            }

            if (request.Password.Length < 8)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Password must be at least 8 characters."
                });
            }

            bool registered = await _authService.RegisterAsync(
                request.Gmail,
                request.Password
            );

            if (!registered)
            {
                return Conflict(new
                {
                    success = false,
                    message = "Email already exists."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Account created successfully."
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Gmail))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Gmail is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Password is required."
                });
            }

            var token = await _authService.LoginAsync(
                request.Gmail,
                request.Password
            );

            if (token == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid email or password."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Login successful.",
                token = token
            });
        }

        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] RequestOtpDto request)
        {
            await _authService.RequestPasswordResetAsync(
                request.Email
            );

            return Ok(new
            {
                success = true,
                message = "If the account exists, a password reset code has been sent."
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
    [FromBody] ResetPasswordDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Gmail))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Gmail is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Otp))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "OTP is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "New password is required."
                });
            }

            if (request.NewPassword.Length < 8)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Password must be at least 8 characters."
                });
            }

            bool success = await _authService.ResetPasswordAsync(
                request.Gmail,
                request.Otp,
                request.NewPassword
            );

            if (!success)
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
                message = "Password has been reset successfully."
            });
        }
        
        [HttpGet("me")]
        public async Task<IActionResult> Athume()
        {

            return Ok(new
            {
                email = ClaimTypes.Email
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
