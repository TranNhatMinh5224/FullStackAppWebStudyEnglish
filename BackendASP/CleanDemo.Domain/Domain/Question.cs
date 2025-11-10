using CleanDemo.Domain.Enums;

namespace CleanDemo.Domain.Entities
{
    public class Question
    {
        public int QuestionId { get; set; }

        public TypeQuestion Type { get; set; } = TypeQuestion.MultipleChoice;

        public string StemText { get; set; } = string.Empty;
        public string? StemHtml { get; set; }

        public int QuizGroupId { get; set; }
        public int QuizSectionId { get; set; }

        public int Points { get; set; } = 10;
        public int OrderIndex { get; set; }

        // Cho dạng trắc nghiệm
        public string? CorrectAnswer { get; set; }

        public string? Explanation { get; set; }

        // Media đính kèm (ảnh/audio/video)
        public string? MediaUrl { get; set; }
        public string? MediaType { get; set; }

        public string MetadataJson { get; set; } = "{}"; // tuỳ chọn, cho loại câu hỏi đặc biệt

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public QuizSection? QuizSection { get; set; }
        public QuizGroup? QuizGroup { get; set; }
        public List<AnswerOption> Options { get; set; } = new();
        public List<QuizUserAnswer> UserAnswers { get; set; } = new();
    }
}
