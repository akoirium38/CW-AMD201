using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AuthService.API.Services;

namespace AuthService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly GmailService _emailService;

        public EmailController(GmailService gmailService)
        {
            _emailService = gmailService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail(string email)
        {
            await _emailService.SendEmailAsync(
                email,
                "Test",
                "Tested successfully"
            );

            return Ok("Email sent!");
        }
    }
}
