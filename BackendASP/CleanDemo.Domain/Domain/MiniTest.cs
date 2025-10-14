namespace CleanDemo.Domain.Entities;

public class MiniTest
{
    public int MiniTestId { get; set; }
    public string Title { get; set; } = string.Empty;

    public int LessonId { get; set; }
    
    // Navigation Properties
    public Lesson? Lesson { get; set; }
    public List<Question> Questions { get; set; } = new();
}
