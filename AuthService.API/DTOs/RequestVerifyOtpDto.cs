namespace AuthService.API.DTOs
{
    public class RequestVerifyOtpDto
    {
        public string Code { get; set; }
        public string Email { get; set; }
    }
}