using CleanDemo.Application.Interface;

namespace CleanDemo.Infrastructure.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly string _templatePath;

        public EmailTemplateService()
        {
            // Look for templates in the Infrastructure project's Templates folder
            _templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "CleanDemo.Infrastructure", "Templates");
            
            // Fallback to current directory if development path doesn't exist
            if (!Directory.Exists(_templatePath))
            {
                _templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
            }
        }

        public string GenerateOTPEmailTemplate(string otpCode, string userName)
        {
            var templatePath = Path.Combine(_templatePath, "OTPEmail.html");
            
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Email template not found: {templatePath}");
            }

            var template = File.ReadAllText(templatePath);
            
            // Replace placeholders
            return template
                .Replace("{{OTPCode}}", otpCode)
                .Replace("{{UserName}}", userName);
        }

        public string GenerateWelcomeEmailTemplate(string userName)
        {
            // Future template for welcome emails
            return $"<h1>Welcome {userName}!</h1><p>Thank you for joining English Learning App!</p>";
        }

        public string GeneratePasswordChangedEmailTemplate(string userName)
        {
            // Future template for password change notifications
            return $"<h1>Password Changed</h1><p>Hello {userName}, your password has been successfully changed.</p>";
        }
    }
}