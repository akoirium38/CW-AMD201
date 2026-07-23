using AuthService.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.API.Services
{
    public class Auth
    {
        private readonly AuthServiceAPIContext _context;
        private readonly OtpService _otpService;
        private readonly GmailService _gmailService;
        private readonly JwtService _jwtService;

        public Auth(
            AuthServiceAPIContext context,
            OtpService otpService,
            GmailService gmailService,
            JwtService jwtService)
        {
            _context = context;
            _otpService = otpService;
            _gmailService = gmailService;
            _jwtService = jwtService;
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

        public async Task<string?> VerifyOtp(string OtpCode, string gmail)
        {
            bool IsVerified = await _otpService.CheckOtpCode(OtpCode, gmail);

            if (!IsVerified)
            {
                return null;
            }

            var user = await _context.User.FirstOrDefaultAsync(u => u.Gmail == gmail);

            if (user == null)
            {
                return null;
            }

            var token = _jwtService.GenerateToken(user.Id, user.Gmail);

            return token;
        }


    }
}
