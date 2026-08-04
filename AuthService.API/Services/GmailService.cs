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
            var apiKey = _configuration["Mailjet:ApiKey"];
            var secretKey = _configuration["Mailjet:SecretKey"];

            if (string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(secretKey))
            {
                throw new Exception("Mailjet API credentials are missing.");
            }

            var authToken = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{apiKey}:{secretKey}")
            );

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.mailjet.com/v3.1/send"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Basic", authToken);

            var email = new
            {
                Messages = new[]
                {
                    new
                    {
                        From = new
                        {
                            Email = _configuration["Mailjet:FromEmail"],
                            Name = _configuration["Mailjet:FromName"]
                        },
                        To = new[]
                        {
                            new
                            {
                                Email = toEmail
                            }
                        },
                        Subject = subject,
                        TextPart = body
                    }
                }
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
                    $"Failed to send email via Mailjet.\n" +
                    $"Status: {(int)response.StatusCode} {response.StatusCode}\n" +
                    $"Response: {error}"
                );
            }
        }
    }
}