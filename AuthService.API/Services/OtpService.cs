using AuthService.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace AuthService.API.Services
{
    public class OtpService
    {
        private readonly AuthServiceAPIContext _context;

        public OtpService(AuthServiceAPIContext context)
        {
            _context = context;
        }
        public string CreateOtpCode(int length = 6)
        {
            const string chars = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890";

            var Code = new char[length];

            for (int i = 0; i < length; i++)
            {
                Code[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
            }

            string OtpCode = new string(Code);

            return OtpCode;
        }

        public async Task SaveOtpCode(string code, string email)
        {
            OtpCode otpcode = new OtpCode()
            {
                Email = email,
                Code = code,
                ExpiresAt = DateTime.Now.AddMinutes(5),
                IsUsed = false,
            };

            _context.OtpCodes.Add(otpcode);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CheckOtpCode(string code, string email)
        {
            var otpCode = _context.OtpCodes.FirstOrDefault(o => o.Code == code);

            if(otpCode == null  ||otpCode.Email != email)
            {
                return false;
            }

            if(otpCode.ExpiresAt < DateTime.UtcNow || otpCode.IsUsed == true)
            {
                _context.OtpCodes.Remove(otpCode);
                await _context.SaveChangesAsync();
                return false;
            }
            _context.OtpCodes.Remove(otpCode);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task ClearOtpCodes() // to remove all code that used or expired
        {
            var invalidOtps = await _context.OtpCodes
                                .Where(x => x.IsUsed || DateTime.UtcNow > x.ExpiresAt)
                                .ToListAsync();

            _context.OtpCodes.RemoveRange(invalidOtps);

            await _context.SaveChangesAsync();
        }
    }
}
