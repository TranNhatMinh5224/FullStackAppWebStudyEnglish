using CleanDemo.Application.Interface;

namespace CleanDemo.Infrastructure.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly ITemplatePathResolver _pathResolver;

        public EmailTemplateService(ITemplatePathResolver pathResolver)
        {
            _pathResolver = pathResolver;
        }

        public string GenerateOTPEmailTemplate(string otpCode, string userName)
        {
            var templatePath = _pathResolver.GetTemplatePath("OTPEmail.html");

            if (!_pathResolver.TemplateExists("OTPEmail.html"))
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
        public string GenerateNotifyJoinCourseTemplate(string courseName, string userName)
        {
            var templatePath = _pathResolver.GetTemplatePath("CoursePurchaseConfirmation.html");
            var htmlTemplate = File.ReadAllText(templatePath);

            return htmlTemplate
                .Replace("{{USER_NAME}}", userName)
                .Replace("{{COURSE_NAME}}", courseName)
                .Replace("{{PURCHASE_DATE}}", DateTime.Now.ToString("dd/MM/yyyy"))
                .Replace("{{COURSE_URL}}", $"https://catalunya-english.com/courses/{courseName.Replace(" ", "-").ToLower()}")
                .Replace("{{CURRENT_YEAR}}", DateTime.Now.Year.ToString());
        }

        public string GenerateTeacherPackagePurchaseTemplate(string packageName, string userName, decimal price, DateTime validUntil)
        {
            var templatePath = _pathResolver.GetTemplatePath("TeacherPackagePurchase.html");
            var htmlTemplate = File.ReadAllText(templatePath);

            return htmlTemplate
                .Replace("{{USER_NAME}}", userName)
                .Replace("{{PACKAGE_NAME}}", packageName)
                .Replace("{{PRICE}}", price.ToString("F2"))
                .Replace("{{PURCHASE_DATE}}", DateTime.Now.ToString("dd/MM/yyyy"))
                .Replace("{{VALID_UNTIL}}", validUntil.ToString("dd/MM/yyyy"))
                .Replace("{{TEACHER_DASHBOARD_URL}}", "https://catalunya-english.com/teacher/dashboard")
                .Replace("{{CURRENT_YEAR}}", DateTime.Now.Year.ToString());
        }
    }
}