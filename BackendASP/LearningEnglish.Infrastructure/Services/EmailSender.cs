using System.Net;
using System.Net.Mail;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Cofigurations;
using Microsoft.Extensions.Options;

namespace LearningEnglish.Infrastructure.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly SmtpOptions _smtpOptions;

        public EmailSender(IOptions<SmtpOptions> smtpOptions)
        {
            _smtpOptions = smtpOptions.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // Validate required SMTP settings
                if (string.IsNullOrEmpty(_smtpOptions.Host))
                    throw new InvalidOperationException("Smtp:Host configuration is missing");
                if (_smtpOptions.Port <= 0)
                    throw new InvalidOperationException("Smtp:Port configuration is missing or invalid");
                if (string.IsNullOrEmpty(_smtpOptions.User))
                    throw new InvalidOperationException("Smtp:User configuration is missing");
                if (string.IsNullOrEmpty(_smtpOptions.Password))
                    throw new InvalidOperationException("Smtp:Password configuration is missing");

                using var client = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port)
                {
                    Credentials = new NetworkCredential(_smtpOptions.User, _smtpOptions.Password),
                    EnableSsl = _smtpOptions.EnableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpOptions.User, _smtpOptions.FromName),
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
