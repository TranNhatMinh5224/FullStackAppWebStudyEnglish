namespace LearningEnglish.Domain.Entities
{
    // 
    public class QuizSection
    {
        public int QuizSectionId { get; set; }
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        // Navigation Properties
        public Quiz? Quiz { get; set; }
        public List<QuizGroup> QuizGroups { get; set; } = new();
    }
}
