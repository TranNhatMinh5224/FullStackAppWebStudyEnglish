using System.Net;
using System.Net.Mail;
using CleanDemo.Application.Interface;

namespace CleanDemo.Application.Service
{
    public class EmailService : IEmailService
    {
        private readonly IEmailTemplateService _templateService;

        public EmailService(IEmailTemplateService templateService)
        {
            _templateService = templateService;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                Console.WriteLine($"[DEBUG] EmailService - SendEmailAsync called for: {toEmail}");
                
                // Load SMTP settings from environment variables (.env file)
                var host = Environment.GetEnvironmentVariable("SMTP_HOST_ASPELEARNING");
                var portString = Environment.GetEnvironmentVariable("SMTP_PORT_ASPELEARNING");
                var username = Environment.GetEnvironmentVariable("SMTP_USER_ASPELEARNING");
                var password = Environment.GetEnvironmentVariable("SMTP_PASS_ASPELEARNING");
                
                Console.WriteLine($"[DEBUG] EmailService - SMTP Host: {host}, Port: {portString}");
                Console.WriteLine($"[DEBUG] EmailService - SMTP Username: {username}");
                
                // Validate required SMTP settings
                if (string.IsNullOrEmpty(host))
                    throw new InvalidOperationException("SMTP_HOST_ASPELEARNING environment variable not configured");
                if (string.IsNullOrEmpty(portString) || !int.TryParse(portString, out var port))
                    throw new InvalidOperationException("SMTP_PORT_ASPELEARNING environment variable not configured or invalid");
                if (string.IsNullOrEmpty(username))
                    throw new InvalidOperationException("SMTP_USER_ASPELEARNING environment variable not configured");
                if (string.IsNullOrEmpty(password))
                    throw new InvalidOperationException("SMTP_PASS_ASPELEARNING environment variable not configured");
                
                // Gmail SMTP always uses SSL
                var enableSsl = true;

                Console.WriteLine($"[DEBUG] EmailService - Creating SMTP client...");
                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = enableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(username, "E-Learning English Platform"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                Console.WriteLine($"[DEBUG] EmailService - Sending email...");
                await client.SendMailAsync(mailMessage);
                Console.WriteLine($"[DEBUG] EmailService - Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] EmailService Exception: {ex.Message}");
                Console.WriteLine($"[DEBUG] EmailService StackTrace: {ex.StackTrace}");
                throw; // Re-throw để PasswordService có thể catch và xử lý
            }
        }

        public async Task SendOTPEmailAsync(string toEmail, string otpCode, string userName)
        {
            Console.WriteLine($"[DEBUG] EmailService - SendOTPEmailAsync called for: {toEmail}, OTP: {otpCode}, User: {userName}");
            
            var subject = "Your OTP Code - Catalunya English";
            var body = _templateService.GenerateOTPEmailTemplate(otpCode, userName);
            
            Console.WriteLine($"[DEBUG] EmailService - Generated email body with template");
            await SendEmailAsync(toEmail, subject, body);
        }
    }
}
