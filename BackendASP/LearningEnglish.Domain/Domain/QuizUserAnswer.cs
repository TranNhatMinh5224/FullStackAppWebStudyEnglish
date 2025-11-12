namespace LearningEnglish.Domain.Entities
{
    public class QuizUserAnswer
    {
        public int QuizUserAnswerId { get; set; }
        public int QuizAttemptId { get; set; }
        public int UserId { get; set; }      // để truy vấn nhanh theo user
        public int QuestionId { get; set; }

        // MCQ/TrueFalse (chọn 1)
        public int? SelectedOptionId { get; set; } // Lưu đáp án đã chọn

        public string? AnswerDataJson { get; set; } // Lưu trữ dữ liệu câu trả lời dạng JSON (cho câu điền khuyết, tự luận, sắp xếp, ghép đôi, nhiều đáp án…)

        // Kết quả chấm
        public bool? IsCorrect { get; set; }      // câu trả lời đúng/sai
        public decimal MaxPoints { get; set; }    // lưu lại điểm tối đa tại thời điểm attempt
        public decimal PointsEarned { get; set; } // điểm đạt được 

       
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow; // thời gian trả lời
        // public int TimeSpentSeconds { get; set; } = 0;  

        // Navigation
        public QuizAttempt QuizAttempt { get; set; } = null!;
        public User User { get; set; } = null!;
        public Question Question { get; set; } = null!;
        public AnswerOption? SelectedOption { get; set; }
        public List<QuizUserAnswerOption> SelectedOptions { get; set; } = new(); 
    }
}
