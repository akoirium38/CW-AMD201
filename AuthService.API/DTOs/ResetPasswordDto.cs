namespace AuthService.API.DTOs
{
    public class ResetPasswordDto
    {
        public string Gmail { get; set; } = string.Empty;

        public string Otp { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;
    }
}
