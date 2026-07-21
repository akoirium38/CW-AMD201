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

        [HttpPost("create_otp")]
        public IActionResult CreateOtpCode()
        {
            string otp = _otpService.CreateOtpCode();

            return Ok(new { otp = otp });
        }
    }
}
