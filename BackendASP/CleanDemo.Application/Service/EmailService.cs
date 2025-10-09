using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace CleanDemo.Application.Service
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpSettings = _configuration.GetSection("Smtp");
            var host = smtpSettings["Host"];
            var portString = smtpSettings["Port"] ?? throw new InvalidOperationException("SMTP Port not configured");
            var port = int.Parse(portString);
            var username = smtpSettings["Username"] ?? throw new InvalidOperationException("SMTP Username not configured");
            var password = smtpSettings["Password"];
            var enableSslString = smtpSettings["EnableSsl"] ?? "true";
            var enableSsl = bool.Parse(enableSslString);

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(username),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }

        public async Task SendOTPEmailAsync(string toEmail, string otpCode, string userName)
        {
            var subject = "Your OTP Code - English Learning App";
            var body = GenerateOTPEmailTemplate(otpCode, userName);
            await SendEmailAsync(toEmail, subject, body);
        }

        private string GenerateOTPEmailTemplate(string otpCode, string userName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Your OTP Code</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background: #f4f4f4; }}
        .header {{ background: linear-gradient(135deg, #FFCB08 0%, #FFA000 100%); color: white; padding: 30px; text-align: center; border-radius: 12px 12px 0 0; }}
        .content {{ padding: 40px 30px; background: #ffffff; border-radius: 0 0 12px 12px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .otp-container {{ background: #f8f9fa; border: 2px dashed #FFA000; padding: 30px; text-align: center; margin: 25px 0; border-radius: 8px; }}
        .otp-code {{ font-size: 36px; font-weight: bold; color: #495057; letter-spacing: 8px; font-family: 'Courier New', monospace; }}
        .warning {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 4px; }}
        .footer {{ text-align: center; font-size: 12px; color: #6c757d; margin-top: 20px; padding: 20px; }}
        .highlight {{ color: #dc3545; font-weight: bold; }}
        .icon {{ font-size: 48px; margin-bottom: 10px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='icon'>🔐</div>
            <h1>Password Reset OTP</h1>
            <p>English Learning App</p>
        </div>
        <div class='content'>
            <p>Hello <strong>{userName}</strong>,</p>
            <p>We received a request to reset your password. Please use the following <strong>6-digit OTP code</strong> to proceed:</p>
            
            <div class='otp-container'>
                <p style='margin: 0; font-size: 14px; color: #6c757d; margin-bottom: 10px;'>Your OTP Code</p>
                <div class='otp-code'>{otpCode}</div>
                <p style='margin: 10px 0 0 0; font-size: 12px; color: #6c757d;'>Enter this code in the app</p>
            </div>

            <div class='warning'>
                <p><strong>⏰ Important Security Information:</strong></p>
                <ul style='margin: 10px 0; padding-left: 20px;'>
                    <li>This OTP code will expire in <span class='highlight'>15 minutes</span></li>
                    <li>Use this code only once</li>
                    <li>Never share this code with anyone</li>
                    <li>If you didn't request this, please ignore this email</li>
                </ul>
            </div>

            <p>After entering the OTP code, you'll be able to set a new password for your account.</p>
            
            <hr style='margin: 30px 0; border: none; border-top: 1px solid #dee2e6;'>
            <p style='font-size: 14px; color: #6c757d;'>
                <strong>Didn't request this?</strong> Your account is safe. Someone may have entered your email address by mistake.
            </p>
        </div>
        <div class='footer'>
            <p><strong>English Learning App Team</strong></p>
            <p>This is an automated email, please don't reply.</p>
            <p>Need help? Contact us at support@englishlearningapp.com</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
