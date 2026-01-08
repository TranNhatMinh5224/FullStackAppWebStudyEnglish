namespace LearningEnglish.Application.Interface
{
    public interface IEmailTemplateService
    {
        // Tạo template email OTP
        string GenerateOTPEmailTemplate(string otpCode, string userName);
        
        // Tạo template email chào mừng
        string GenerateWelcomeEmailTemplate(string userName);
        
        // Tạo template email đổi mật khẩu
        string GeneratePasswordChangedEmailTemplate(string userName);
        
        // Tạo template thông báo tham gia course
        string GenerateNotifyJoinCourseTemplate(string courseName, string userName);
        
        // Tạo template mua gói giáo viên
        string GenerateTeacherPackagePurchaseTemplate(string packageName, string userName, decimal price, DateTime validUntil);
        
        // Tạo template nhắc nhở ôn tập
        string GenerateVocabularyReminderTemplate(string studentName, int dueCount);
        
        // Tạo template nhắc nhở streak sắp đứt
        string GenerateStreakReminderTemplate(string userName, int currentStreak, int longestStreak);
    }
}
