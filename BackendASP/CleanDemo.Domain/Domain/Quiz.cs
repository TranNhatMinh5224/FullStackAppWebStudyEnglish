using CleanDemo.Domain.Enums;
namespace CleanDemo.Domain.Entities
{
    public class Quiz
    {
        // thong tin ve bai thi 
        public int QuizId { get; set; }
        public int AssessmentId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Instructions { get; set; } // Hướng dẫn làm bài

        public TypeQuiz Type { get; set; } = TypeQuiz.Practice; 
        public StatusQuiz Status { get; set; } = StatusQuiz.Open;
        public int TotalQuestions { get; set; }
        public int? PassingScore { get; set; }
        public int? OrderIndex { get; set; }
       

        // Han thoi gian lam bai thi

       
        public DateTime? StartTime { get; set; } // Thoi gian bat dau
        public DateTime? EndTime { get; set; } // Thoi gian ket thuc
        

        // Hien thi cau tra loi sau khi nop bai
        public bool? ShowAnswersAfterSubmit { get; set; } = true; // Hien dap an sau khi nop bai

        public bool? ShowScoreImmediately { get; set; } = true; // Hien thi diem so


        // Xao tron cau hoi
        public bool? ShuffleQuestions { get; set; } = true;
        public bool? ShuffleAnswers { get; set; } = true;

        // Practice settings (for vocab exercises, grammar practice)
        public bool? AllowUnlimitedAttempts { get; set; } = false; // Cho phép làm lại không giới hạn
        public int? MaxAttempts { get; set; } // Số lần làm tối đa
        public bool? ShowCorrectAnswersDuringAttempt { get; set; } = false; // Hiện đáp án ngay khi làm (cho practice)


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        // Navigation Properties
        public Assessment? Assessment { get; set; }
        public List<QuizSection> QuizSections { get; set; } = new();
        public List<QuizAttempt> Attempts { get; set; } = new();


    }
}

