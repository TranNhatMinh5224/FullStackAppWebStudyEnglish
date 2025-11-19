namespace LearningEnglish.Domain.Entities;

public class Lesson
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CourseId { get; set; }
    public int OrderIndex { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    


    // Navigation Properties
    public Course? Course { get; set; }
    public List<Module> Modules { get; set; } = new();
    public List<LessonCompletion> LessonCompletions { get; set; } = new();
}
