using LearningEnglish.Domain.Enums;
namespace LearningEnglish.Domain.Entities
{
    public class Quiz
    {
        // thong tin ve bai thi 
        public int QuizId { get; set; }
        public int AssessmentId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Instructions { get; set; } // Hướng dẫn làm bài

        public QuizType Type { get; set; } = QuizType.Practice;
        public QuizStatus Status { get; set; } = QuizStatus.Open;
        public int TotalQuestions { get; set; } // Tong so cau hoi trong bai thi
        public int? PassingScore { get; set; } // Diem dat yeu cau
        public decimal TotalPossibleScore { get; set; } // Tổng điểm tối đa của bài quiz





        // Han thoi gian lam bai thi
        public int? Duration { get; set; } // Thoi gian lam bai (phut)
        public DateTime? AvailableFrom { get; set; } // Thoi gian bat dau co the lam bai


        // Hien thi cau tra loi sau khi nop bai
        public bool? ShowAnswersAfterSubmit { get; set; } = true; // Hien dap an sau khi nop bai

        public bool? ShowScoreImmediately { get; set; } = true; // Hien thi diem so


        // Xao tron cau hoi
        public bool? ShuffleQuestions { get; set; } = true;
        public bool? ShuffleAnswers { get; set; } = true;

        // Practice settings (for vocab exercises, grammar practice)
        public int? MaxAttempts { get; set; } // Số lần làm tối đa



        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        // Navigation Properties
        public Assessment? Assessment { get; set; }
        public List<QuizSection> QuizSections { get; set; } = new();
        public List<QuizAttempt> Attempts { get; set; } = new();


    }
}

