namespace LearningEnglish.Domain.Enums
{
    public enum NotificationType
    {
        CourseEnrollment = 1,    // Đăng ký khóa học thành công
        CourseCompletion = 2 ,    // Hoàn thành khóa học
        VocabularyReminder = 3 ,  // Nhắc nhở ôn từ vựng
        AssessmentGraded = 4 ,    // Nộp bài essay/quiz thành công
        PaymentSuccess = 5 ,      // Thanh toán thành công
        StreakReminder = 6       // Nhắc nhở streak sắp đứt
    }
}