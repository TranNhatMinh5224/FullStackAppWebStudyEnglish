namespace CleanDemo.Domain.Entities;

// Theo dõi tiến trình hoàn thành từng lesson (bài học Grammar) của học sinh
public class LessonCompletion
{
    public int LessonCompletionId { get; set; }
    public int UserId { get; set; }
    public int LessonId { get; set; }
    // % hoàn thành lesson (0-100)
    public float CompletionPercentage { get; set; } = 0;

    // Lesson có hoàn thành không (xem hết video + làm bài tập)
    public bool IsCompleted { get; set; } = false;

    // % video đã xem (0-100)
    public float VideoProgressPercentage { get; set; } = 0;

    // Thời gian xem video cuối cùng (giây)
    public int LastWatchedPositionSeconds { get; set; } = 0;

    // Tổng thời gian học lesson (giây)
    public int TotalTimeSpentSeconds { get; set; } = 0;

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }



    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User User { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
    public ModuleCompletion? ModuleCompletion { get; set; } /// ? 
}
