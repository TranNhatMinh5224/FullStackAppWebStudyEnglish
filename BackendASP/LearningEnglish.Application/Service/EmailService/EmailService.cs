using LearningEnglish.Application.Interface;

namespace LearningEnglish.Application.Service
{
    public class EmailService : IEmailService
    {
        private readonly IEmailTemplateService _templateService;
        private readonly IEmailSender _emailSender;

        public EmailService(IEmailTemplateService templateService, IEmailSender emailSender)
        {
            _templateService = templateService;
            _emailSender = emailSender;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            await _emailSender.SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendOTPEmailAsync(string toEmail, string otpCode, string userName)
        {
            var subject = "Your OTP Code - Catalunya English";
            var body = _templateService.GenerateOTPEmailTemplate(otpCode, userName);

            Console.WriteLine($"[DEBUG] EmailService - Generated email body with template");
            await _emailSender.SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendNotifyJoinCourseAsync(string toEmail, string courseName, string userName)
        {
            var subject = "Course Enrollment Confirmation - Catalunya English";
            var body = _templateService.GenerateNotifyJoinCourseTemplate(courseName, userName);
            await _emailSender.SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendNotifyPurchaseTeacherPackageAsync(string toEmail, string packageName, string userName, decimal price, DateTime validUntil)
        {
            var subject = "Teacher Package Purchase Confirmation - Catalunya English";
            var body = _templateService.GenerateTeacherPackagePurchaseTemplate(packageName, userName, price, validUntil);
            await _emailSender.SendEmailAsync(toEmail, subject, body);
        }
    }
}
