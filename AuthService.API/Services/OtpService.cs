using AuthService.API.Models;
using MongoDB.Driver;
using System.Security.Cryptography;

namespace AuthService.API.Services
{
    public class OtpService
    {
        private readonly AuthDbContext _context;
        public OtpService(AuthDbContext context)
        {
            _context = context;
        }
        // Generate a random OTP code
        public string CreateOtpCode(int length = 6)
        {
            const string chars =
                "qwertyuiopasdfghjklzxcvbnm" +
                "QWERTYUIOPASDFGHJKLZXCVBNM" +
                "1234567890";
            var code = new char[length];
            for (int i = 0; i < length; i++)
            {
                code[i] = chars[
                    RandomNumberGenerator.GetInt32(chars.Length)
                ];
            }

            return new string(code);
        }


        // Save OTP to MongoDB
        public async Task SaveOtpCode(string code, string email)
        {
            var otpCode = new OtpCode
            {
                Email = email,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            };

            await _context.OtpCodes.InsertOneAsync(otpCode);
        }


        // Check OTP
        public async Task<bool> CheckOtpCode(
            string code,
            string email)
        {
            // Find the OTP that matches both
            // the code AND the email
            var otpCode = await _context.OtpCodes
                .Find(x =>
                    x.Code == code &&
                    x.Email == email)
                .FirstOrDefaultAsync();

            // OTP doesn't exist
            if (otpCode == null)
            {
                return false;
            }

            // OTP has expired
            if (otpCode.ExpiresAt < DateTime.UtcNow)
            {
                await _context.OtpCodes.DeleteOneAsync(
                    x => x.Id == otpCode.Id
                );

                return false;
            }

            // OTP has already been used
            if (otpCode.IsUsed)
            {
                await _context.OtpCodes.DeleteOneAsync(
                    x => x.Id == otpCode.Id
                );

                return false;
            }

            // OTP is valid.
            // Delete it so it cannot be reused.
            await _context.OtpCodes.DeleteOneAsync(
                x => x.Id == otpCode.Id
            );

            return true;
        }


        // Remove all used or expired OTP codes
        public async Task ClearOtpCodes()
        {
            var result = await _context.OtpCodes.DeleteManyAsync(
                x =>
                    x.IsUsed ||
                    x.ExpiresAt < DateTime.UtcNow
            );
        }
    }
}