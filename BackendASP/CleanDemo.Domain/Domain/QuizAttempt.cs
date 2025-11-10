using CleanDemo.Domain.Enums;

namespace CleanDemo.Domain.Entities
{

    // Lần làm bài quiz/test của học sinh
    // Dùng cho: quiz trong lesson, quiz trong module, quiz tổng hợp
    public class QuizAttempt
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public int UserId { get; set; }

        // Lần thử thứ mấy (1, 2, 3...)
        public int AttemptNumber { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SubmittedAt { get; set; }
        public StatusQuizAttempt Status { get; set; } = StatusQuizAttempt.InProgress;

        public int Score { get; set; }
        public int MaxScore { get; set; }
        public double Percentage { get; set; }
        public bool IsPassed { get; set; }

        public int TimeSpentSeconds { get; set; }

        // JSON backup của tất cả câu trả lời
        public string? AnswersSnapshot { get; set; }

        // Review từ giáo viên (cho essay questions, assignments)
        public DateTime? ReviewedAt { get; set; }
        public string? TeacherFeedback { get; set; }
        public int? ReviewedBy { get; set; }

        // Navigation Properties
        public Quiz Quiz { get; set; } = null!;
        public User User { get; set; } = null!;
        public User? Reviewer { get; set; }
        public List<QuizUserAnswer> Answers { get; set; } = new();
    }
}
