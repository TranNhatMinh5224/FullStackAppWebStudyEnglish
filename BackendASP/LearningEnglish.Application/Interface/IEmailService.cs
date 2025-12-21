namespace LearningEnglish.Application.Interface
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendOTPEmailAsync(string toEmail, string otpCode, string userName);
        Task SendNotifyJoinCourseAsync(string toEmail, string courseName, string userName);
        Task SendNotifyPurchaseTeacherPackageAsync(string toEmail, string packageName, string userName, decimal price, DateTime validUntil);
        Task SendVocabularyReminderEmailAsync(string toEmail, string studentName, int dueCount);
    }
}
