namespace LearningEnglish.Domain.Entities
{
    public class QuizGroup
    {
        public int QuizGroupId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int QuizSectionId { get; set; }
        public float SumScore { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public List<Question> Questions { get; set; } = new();
        public QuizSection? QuizSection { get; set; }
        public List<MediaAsset> QuizQuestionGroupMedias { get; set; } = new();
    }
}
