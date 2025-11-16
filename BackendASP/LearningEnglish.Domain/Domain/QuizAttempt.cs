using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Domain.Entities
{
    public class QuizAttempt
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public int UserId { get; set; }
        public int AttemptNumber { get; set; } // Lần làm bài thứ mấy

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SubmittedAt { get; set; }
        public QuizAttemptStatus Status { get; set; } = QuizAttemptStatus.InProgress;

        public int TimeSpentSeconds { get; set; } // Thời gian làm bài
        public decimal TotalScore { get; set; } = 0; // Điểm tổng, update real-time

        // JSON lưu điểm từng câu (ví dụ: {"1": 5, "2": 0}) để sum nhanh
        public string? ScoresJson { get; set; }

        public Quiz Quiz { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}