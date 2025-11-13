using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Domain.Entities
{
    // QuizAttempt - Lưu trữ thông tin về lần làm bài quiz của học viên
    public class QuizAttempt
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public int UserId { get; set; }

        public int AttemptNumber { get; set; } // Lần làm bài thứ mấy

        public DateTime StartedAt { get; set; } = DateTime.UtcNow; // Thời gian bắt đầu làm bài 
        public DateTime? SubmittedAt { get; set; } // Thời gian nộp bài 
        public QuizAttemptStatus Status { get; set; } = QuizAttemptStatus.InProgress; // Trạng thái bài làm

        public int TimeSpentSeconds { get; set; } // Tổng thời gian làm bài 

       

      
        public string? AnswersSnapshot { get; set; } // Lưu trữ toàn bộ câu trả lời dạng JSON

        // Navigation
        public Quiz Quiz { get; set; } = null!; // Thuộc về Quiz nào
        public User User { get; set; } = null!; // Học viên làm bài
        public List<QuizUserAnswer> Answers { get; set; } = new(); // Câu trả lời chi tiết
    }
}
