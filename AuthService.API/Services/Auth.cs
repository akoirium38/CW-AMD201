using AuthService.API.Models;
using MongoDB.Driver;
using Org.BouncyCastle.Crypto.Generators;

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

        public async Task<bool> RegisterAsync(
            string gmail,
            string password)
        {
            // Check whether the email already exists
            var existingUser = await _context.Users
                .Find(u => u.Gmail == gmail)
                .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                return false;
            }

            // Hash password
            string _password =
                BCrypt.Net.BCrypt.HashPassword(password);

            // Create user
            var user = new User
            {
                Gmail = gmail,
                Password = _password
            };

            // Save user
            await _context.Users.InsertOneAsync(user);

            return true;
        }

        public async Task<string?> LoginAsync(string gmail,string password)
        {
            // Find user
            var user = await _context.Users
                .Find(u => u.Gmail == gmail)
                .FirstOrDefaultAsync();

            // User doesn't exist
            if (user == null)
            {
                return null;
            }

            // Check password
            bool passwordValid =
                global::BCrypt.Net.BCrypt.Verify(
                    password,
                    user.Password
                );

            // Password incorrect
            if (!passwordValid)
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
        public async Task<bool> RequestPasswordResetAsync(string gmail)
        {
            // Find user
            var user = await _context.Users
                .Find(u => u.Gmail == gmail)
                .FirstOrDefaultAsync();

            // Don't reveal whether the email exists
            // This prevents account enumeration.
            if (user == null)
            {
                return true;
            }

            // Generate OTP
            string otpCode = _otpService.CreateOtpCode();

            // Save OTP
            await _otpService.SaveOtpCode(
                otpCode,
                gmail
            );

            // Email
            string subject = "FileHub Password Reset";

            string body =
                "Hello," +
                "\n\n" +
                "Your FileHub password reset code is: " + otpCode +
                "\n\n" +
                "This code will expire after 5 minutes." +
                "\n\n" +
                "If you did not request a password reset, you can ignore this email." +
                "\n\n" +
                "Regards," +
                "\nFileHub";

            await _gmailService.SendEmailAsync(
                gmail,
                subject,
                body
            );

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string gmail,string otp,string newPassword)
        {
            // Find user
            var user = await _context.Users
                .Find(u => u.Gmail == gmail)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return false;
            }

            // Verify OTP
            bool otpValid =
                await _otpService.CheckOtpCode(
                    otp,
                    gmail
                );

            if (!otpValid)
            {
                return false;
            }

            // Hash new password
            string passwordHash =
                global::BCrypt.Net.BCrypt.HashPassword(
                    newPassword
                );

            // Update password
            var update = Builders<User>.Update
                .Set(u => u.Password, passwordHash);

            var result = await _context.Users.UpdateOneAsync(
                u => u.Id == user.Id,
                update
            );

            return result.ModifiedCount > 0;
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