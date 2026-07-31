using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AuthService.API.Services
{
    public class GmailService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GmailService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var apiKey = _configuration["Resend:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("Resend API key is missing.");

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.resend.com/emails"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var email = new
            {
                from = $"{_configuration["Resend:FromName"]} <{_configuration["Resend:From"]}>",
                to = new[] { toEmail },
                subject = subject,
                text = body
            };

            request.Content = new StringContent(
                JsonSerializer.Serialize(email),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new Exception(
                    $"Failed to send email via Resend.\n" +
                    $"Status: {(int)response.StatusCode} {response.StatusCode}\n" +
                    $"Response: {error}"
                );
            }

        }
    }
}
