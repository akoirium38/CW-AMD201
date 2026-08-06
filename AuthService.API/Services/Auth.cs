using AuthService.API.Models;
using MongoDB.Driver;

namespace AuthService.API.Services
{
    public class Auth
    {
        private readonly AuthDbContext _context;
        private readonly OtpService _otpService;
        private readonly GmailService _gmailService;
        private readonly JwtService _jwtService;

        public Auth(
            AuthDbContext context,
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
            // Find user in MongoDB
            var user = await _context.Users
                .Find(u => u.Gmail == gmail)
                .FirstOrDefaultAsync();

            // If user doesn't exist, create a new user
            if (user == null)
            {
                user = new User
                {
                    Gmail = gmail
                };

                // Insert user into MongoDB
                await _context.Users.InsertOneAsync(user);
            }

            // Generate OTP
            string otpCode = _otpService.CreateOtpCode();

            // Save OTP to MongoDB
            await _otpService.SaveOtpCode(otpCode, gmail);

            // Email information
            string subject = "OTP Code";

            string body =
                "Hello," +
                "\nThis is your OTP code: " + otpCode +
                "\nThis code will expire after 5 minutes."+
                "\nRegards," +
                "\nFileHub";

            // Send OTP email
            await _gmailService.SendEmailAsync(
                gmail,
                subject,
                body
            );
        }


        public async Task<string?> VerifyOtp(
            string otpCode,
            string gmail)
        {
            // Check OTP
            bool isVerified =
                await _otpService.CheckOtpCode(
                    otpCode,
                    gmail
                );

            // OTP is invalid
            if (!isVerified)
            {
                return null;
            }

            // Find user in MongoDB
            var user = await _context.Users
                .Find(u => u.Gmail == gmail)
                .FirstOrDefaultAsync();

            // User doesn't exist
            if (user == null)
            {
                return null;
            }

            // Generate JWT
            var token = _jwtService.GenerateToken(
                user.Id,
                user.Gmail
            );

            return token;
        }
    }
}