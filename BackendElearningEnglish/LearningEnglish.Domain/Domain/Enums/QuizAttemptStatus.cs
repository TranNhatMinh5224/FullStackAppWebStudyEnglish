namespace LearningEnglish.Domain.Enums
{
    public enum QuizAttemptStatus
    {
        InProgress = 1, // Đang làm bài
        Submitted = 2, // Đã nộp bài
        Graded = 3, // Đã chấm điểm
        TimeExpired = 4, // Hết thời gian làm bài
        Abandoned = 5 // Bỏ dở
    }
}
