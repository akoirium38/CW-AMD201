namespace FileService.API.DTOs
{
    // Request payload for verifying file password before download
    public class VerifyPasswordRequestDto
    {
        public string Password { get; set; } = string.Empty;
    }
}
