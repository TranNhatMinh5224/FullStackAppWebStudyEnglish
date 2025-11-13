
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Domain.Entities
{
    public class Question
    {
        public int QuestionId { get; set; }

        public QuestionType Type { get; set; } = QuestionType.MultipleChoice;
        public string StemText { get; set; } = string.Empty; // Câu hỏi dạng text thuần
        public string? StemHtml { get; set; } // Câu hỏi dạng  text/HTML

        public int QuizGroupId { get; set; } // Thuộc về QuizGroup nào
        public int QuizSectionId { get; set; } // Thuộc về QuizSection nào

        public decimal Points { get; set; } = 10m; // Điểm câu hỏi
        public ScoringStrategy Scoring { get; set; } = ScoringStrategy.AllOrNothing; // Chiến lược chấm điểm



        public string? CorrectAnswersJson { get; set; } // Đáp án đúng (cho câu nhiều đáp án, ghép đôi, sắp xếp) 

        public string? Explanation { get; set; } // Giải thích đáp án

        // Media cho câu hỏi (ảnh/audio/video)
        public string? MediaUrl { get; set; }
        public string? MediaType { get; set; }

        // Cấu hình đặc thù theo Type (matching/order/fillblank nhiều ô…)
        public string MetadataJson { get; set; } = "{}"; // Lưu trữ dữ liệu cấu hình dạng JSON 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public QuizSection? QuizSection { get; set; }
        public QuizGroup? QuizGroup { get; set; }
        public List<AnswerOption> Options { get; set; } = new();
        public List<QuizUserAnswer> UserAnswers { get; set; } = new();
    }
}
