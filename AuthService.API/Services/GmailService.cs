using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AuthService.API.Services
{
    public class GmailService
    {
        private readonly IConfiguration _configuration;

        public GmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(String toEmail, string subject, string body)
        {
            var email = new MimeMessage();

            email.From.Add(
                new MailboxAddress(
                    "FileHub:AMD201", _configuration["Gmail:Username"])
            );

            email.To.Add(
                MailboxAddress.Parse(toEmail)
            );

            email.Subject = subject;
            email.Body = new TextPart("plain")
            {
                Text = body
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                "smtp.gmail.com",
                587,
                SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(
                _configuration["Gmail:UserName"],
                _configuration["Gmail:AppPassword"]
            );

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);


        }
    }
}
