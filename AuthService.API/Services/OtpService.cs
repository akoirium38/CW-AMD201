using System.Security.Cryptography;

namespace AuthService.API.Services
{
    public class OtpService
    {
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
    }
}
