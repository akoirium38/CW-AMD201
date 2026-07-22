using AuthService.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.API.Services
{
    public class Auth
    {
        private readonly AuthServiceAPIContext _context;
        private readonly OtpService _otpService;
        private readonly GmailService _gmailService;

        public Auth(
            AuthServiceAPIContext context,
            OtpService otpService, GmailService gmailService)
        {
            _context = context;
            _otpService = otpService;
            _gmailService = gmailService;
        }

        public async Task RequestOtpAsync(string gmail)
        {
            //find user
            var user = await _context.User.FirstOrDefaultAsync(u => u.Gmail == gmail);

            //if not create user
            if (user == null)
            {
                user = new User()
                {
                    Gmail = gmail,
                };

                _context.User.Add(user);

                await _context.SaveChangesAsync();
            }

            //otp
            string OtpCode = _otpService.CreateOtpCode();

            await _otpService.SaveOtpCode(OtpCode, gmail);

            string subject = "OTP code";

            string body = "this is your OTP code: " + OtpCode + "\nthis code will be expire after 5 mins";

            //send mail
            await _gmailService.SendEmailAsync(gmail,subject,body);
        }
    }
}
