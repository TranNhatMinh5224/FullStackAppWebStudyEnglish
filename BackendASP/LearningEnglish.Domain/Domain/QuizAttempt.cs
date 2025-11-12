using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Domain.Entities
{
    // QuizAttempt - Lưu trữ thông tin về lần làm bài quiz của học viên
    public class QuizAttempt
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public int UserId { get; set; }

        public int AttemptNumber { get; set; } = 1; // Lần làm bài thứ mấy

        public DateTime StartedAt { get; set; } = DateTime.UtcNow; // Thời gian bắt đầu làm bài 
        public DateTime? SubmittedAt { get; set; } // Thời gian nộp bài 
        public StatusQuizAttempt Status { get; set; } = StatusQuizAttempt.InProgress; // Trạng thái bài làm

        public decimal Score { get; set; } // Điểm số đạt được
        public decimal MaxScore { get; set; } // Điểm tối đa
        public decimal Percentage { get; set; } // Tỷ lệ phần trăm
        public bool IsPassed { get; set; } // Đã vượt qua hay chưa

        public int TimeSpentSeconds { get; set; } // Tổng thời gian làm bài 

        public string? ShuffleSeedJson { get; set; } // Dùng để tái lập thứ tự đã xáo trộn câu/đáp án

        // Snapshot toàn bộ câu trả lời (backup)
        public string? AnswersSnapshot { get; set; } // Lưu trữ toàn bộ câu trả lời dạng JSON


        public DateTime? ReviewedAt { get; set; } // Thời gian giáo viên chấm bài
        public string? TeacherFeedback { get; set; } // Phản hồi của giáo viên
        public int? ReviewedBy { get; set; } // Ai đã chấm bài

        // Navigation
        public Quiz Quiz { get; set; } = null!; // Thuộc về Quiz nào
        public User User { get; set; } = null!; // Học viên làm bài
        public User? Reviewer { get; set; } // Giáo viên chấm bài
        public List<QuizUserAnswer> Answers { get; set; } = new(); // Câu trả lời chi tiết
    }
}
