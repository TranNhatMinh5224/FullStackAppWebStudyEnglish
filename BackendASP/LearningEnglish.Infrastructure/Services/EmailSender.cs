using System.Net;
using System.Net.Mail;
using LearningEnglish.Application.Interface;
using Microsoft.Extensions.Configuration;

namespace LearningEnglish.Infrastructure.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // Load SMTP settings from appsettings.json
                var host = _configuration["Smtp:Host"];
                var portString = _configuration["Smtp:Port"];
                var username = _configuration["Smtp:User"];
                var password = _configuration["Smtp:Password"];
                var enableSslString = _configuration["Smtp:EnableSsl"];
                var fromName = _configuration["Smtp:FromName"] ?? "E-Learning English Platform";

                // Validate required SMTP settings
                if (string.IsNullOrEmpty(host))
                    throw new InvalidOperationException("Smtp:Host configuration is missing");
                if (string.IsNullOrEmpty(portString) || !int.TryParse(portString, out var port))
                    throw new InvalidOperationException("Smtp:Port configuration is missing or invalid");
                if (string.IsNullOrEmpty(username))
                    throw new InvalidOperationException("Smtp:User configuration is missing");
                if (string.IsNullOrEmpty(password))
                    throw new InvalidOperationException("Smtp:Password configuration is missing");

                // Parse EnableSsl (default true for Gmail)
                var enableSsl = string.IsNullOrEmpty(enableSslString) || bool.Parse(enableSslString);

                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = enableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(username, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);

            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi gửi email: {ex.Message}", ex);
            }
        }
    }
}
