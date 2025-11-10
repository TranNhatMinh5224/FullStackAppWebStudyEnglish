namespace LearningEnglish.Domain.Entities
{
    
    public class QuizUserAnswerOption
    {
        public int QuizUserAnswerId { get; set; }
        public int AnswerOptionId { get; set; }

        // Navigation
        public QuizUserAnswer QuizUserAnswer { get; set; } = null!;
        public AnswerOption AnswerOption { get; set; } = null!;
    }
}
