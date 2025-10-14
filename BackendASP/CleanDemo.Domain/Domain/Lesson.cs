namespace CleanDemo.Domain.Entities;

public class Lesson
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Foreign Key - Liên kết trực tiếp với Course
    public int CourseId { get; set; }

    // Navigation Properties
    public Course? Course { get; set; }
    public List<Vocabulary> Vocabularies { get; set; } = new();
    public List<MiniTest> MiniTests { get; set; } = new();
    public List<Progress> ProgressRecords { get; set; } = new();
}