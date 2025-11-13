namespace LearningEnglish.Domain.Entities
{
    // QuizAttemptResult - Lưu trữ kết quả chấm điểm của attempt
    public class QuizAttemptResult
    {
        public int ResultId { get; set; }
        public int AttemptId { get; set; } // FK to QuizAttempt

        // Kết quả chấm điểm tự động
        public decimal Score { get; set; } // Điểm số đạt được
        public decimal MaxScore { get; set; } // Điểm tối đa
        public decimal Percentage { get; set; } // Tỷ lệ phần trăm
        public bool IsPassed { get; set; } // Đã vượt qua hay chưa
        public DateTime ScoredAt { get; set; } = DateTime.UtcNow; // Thời gian chấm điểm

        // Chấm điểm thủ công bởi giáo viên (optional)
        public decimal? ManualScore { get; set; } // Điểm do teacher chấm (nếu có câu tự luận)
        public DateTime? ReviewedAt { get; set; } // Thời gian giáo viên chấm bài
        public string? TeacherFeedback { get; set; } // Phản hồi của giáo viên
        public int? ReviewedBy { get; set; } // User ID của giáo viên chấm bài
        public DateTime? FinalizedAt { get; set; } // Thời gian hoàn tất chấm điểm

        // Không thêm Navigation properties - để entity độc lập
    }
}
