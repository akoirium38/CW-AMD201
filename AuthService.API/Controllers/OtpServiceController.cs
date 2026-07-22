using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using AuthService.API.Services;

namespace AuthService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OtpServiceController : ControllerBase
    {
        private readonly OtpService _otpService;

        public OtpServiceController(OtpService otpService)
        {
            _otpService = otpService;
        }

        [HttpPost("AddOtpCode")]
        public async Task<IActionResult> OtpCode(string email)
        {
            string OtpCode = _otpService.CreateOtpCode();

            await _otpService.SaveOtpCode(OtpCode, email);

            return Ok("created successfully");
        }

        [HttpPost("ClearOtpCode")]
        public async Task<IActionResult> ClearOtpCodes()
        {
            await _otpService.ClearOtpCodes();

            return Ok("Cleared all used and expired codes");
        }
    }
}
