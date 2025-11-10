namespace LearningEnglish.Domain.Entities
{
    public class QuizUserAnswer
    {
        public int QuizUserAnswerId { get; set; }
        public int QuizAttemptId { get; set; }
        public int UserId { get; set; }          // để truy vấn nhanh theo user
        public int QuestionId { get; set; }

        // Dành cho MCQ/TrueFalse (chọn 1)
        public int? SelectedOptionId { get; set; }

        // Dành cho MultipleAnswers / các dạng phức tạp (FillBlank/Matching/Ordering/ShortAnswer)
        // Lưu JSON: ví dụ { "blanks":[{"index":1,"text":"goes"}], "pairs":[["dog","con chó"]], "order":[2,1,3,4], "text":"cold" }
        public string? AnswerDataJson { get; set; }

        // Kết quả chấm cho câu này
        public bool? IsCorrect { get; set; }     // null = chưa chấm
        public decimal MaxPoints { get; set; }   // Changed from int to decimal for consistency
        public decimal PointsEarned { get; set; } // Changed from int to decimal for consistency

        public string? Feedback { get; set; }    // phản hồi riêng từng câu (nếu có)
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
        public int TimeSpentSeconds { get; set; } = 0;

        // Navigation
        public QuizAttempt QuizAttempt { get; set; } = null!;
        public User User { get; set; } = null!;
        public Question Question { get; set; } = null!;
        public AnswerOption? SelectedOption { get; set; }
        public List<QuizUserAnswerOption> SelectedOptions { get; set; } = new(); // cho MultipleAnswers
    }
}
