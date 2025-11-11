namespace LearningEnglish.Domain.Entities
{
    public class AnswerOption
    {
        public int AnswerOptionId { get; set; }

        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }


        public int QuestionId { get; set; }

        // Navigation Properties
        public Question? Question { get; set; }
        public List<QuizUserAnswer> UserAnswers { get; set; } = new();
        public List<QuizUserAnswerOption> UserAnswerOptions { get; set; } = new();
    }
}
