namespace LearningEnglish.Domain.Entities
{
    // QuizUserAnswerOption - Lưu trữ các tùy chọn đáp án đã chọn cho câu trả lời của học viên (dùng cho MultipleAnswers, Ordering)
    public class QuizUserAnswerOption
    {
        public int QuizUserAnswerId { get; set; }
        public int AnswerOptionId { get; set; }

        
        public int? SelectedOrder { get; set; }

        // Navigation
        public QuizUserAnswer QuizUserAnswer { get; set; } = null!;
        public AnswerOption AnswerOption { get; set; } = null!;
    }
}
