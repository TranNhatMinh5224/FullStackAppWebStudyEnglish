using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Domain.Entities
{

    public class Module
    {
        public int ModuleId { get; set; }
        public int LessonId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrderIndex { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ModuleType ContentType { get; set; }

        // Navigation Properties
        public Lesson? Lesson { get; set; }
        public List<Lecture> Lectures { get; set; } = new();

        public List<FlashCard> FlashCards { get; set; } = new();
        public List<Assessment> Assessments { get; set; } = new();
        public List<ModuleCompletion> ModuleCompletions { get; set; } = new();

    }
}
