namespace LearningEnglish.Application.Interface
{
    public interface IEmailService
    {
        // Gửi email
        Task SendEmailAsync(string toEmail, string subject, string body);
        
        // Gửi email OTP
        Task SendOTPEmailAsync(string toEmail, string otpCode, string userName);
        
        // Gửi thông báo tham gia course
        Task SendNotifyJoinCourseAsync(string toEmail, string courseName, string userName);
        
        // Gửi thông báo mua gói giáo viên
        Task SendNotifyPurchaseTeacherPackageAsync(string toEmail, string packageName, string userName, decimal price, DateTime validUntil);
        
        // Gửi nhắc nhở ôn tập từ vựng
        Task SendVocabularyReminderEmailAsync(string toEmail, string studentName, int dueCount);
        
        // Gửi nhắc nhở streak sắp đứt
        Task SendStreakReminderEmailAsync(string toEmail, string userName, int currentStreak, int longestStreak);
    }
}
