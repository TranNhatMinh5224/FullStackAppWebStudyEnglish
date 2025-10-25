namespace CleanDemo.Domain.Entities;

public class Progress
{
    public int ProgressId { get; set; }
    public int UserId { get; set; }
    public int LessonId { get; set; }
    public double Completion { get; set; } // 0-100 (%)
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public User? User { get; set; }
    public Lesson? Lesson { get; set; }
}
