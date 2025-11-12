namespace LearningEnglish.Application.DTOs
{
    public class QuizSectionDto
    {
        public int QuizSectionId { get; set; }
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

    }
    public class CreateQuizSectionDto
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
    public class UpdateQuizSectionDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class ListQuizSectionDto
    {
        public int QuizSectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

}