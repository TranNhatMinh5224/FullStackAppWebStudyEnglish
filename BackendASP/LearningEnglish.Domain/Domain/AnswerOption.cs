namespace LearningEnglish.Domain.Entities
{
    public class AnswerOption
    {
        public int AnswerOptionId { get; set; }
        public int QuestionId { get; set; }

        public string Text { get; set; } = string.Empty; // có thể để trống nếu đáp án là ảnh
        public bool IsCorrect { get; set; }

        // Hỗ trợ ImageChoice hoặc đáp án có media
        public string? MediaUrl { get; set; }
        public string? MediaType { get; set; } // "image/png", "audio/mpeg", ...

        public string? Feedback { get; set; }   // phản hồi theo option
        public int OrderIndex { get; set; } = 0; // lưu thứ tự gốc

        // Navigation
        public Question? Question { get; set; }
        public List<QuizUserAnswerOption> UserAnswerOptions { get; set; } = new();
    }
}
