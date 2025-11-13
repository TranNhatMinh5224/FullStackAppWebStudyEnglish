namespace LearningEnglish.Domain.Entities;

// Theo dõi tiến trình hoàn thành từng lesson (bài học Grammar) của học sinh
public class LessonCompletion
{
    public int LessonCompletionId { get; set; }
    public int UserId { get; set; }
    public int LessonId { get; set; }

    // % hoàn thành lesson (0-100) - Auto calculated from modules
    public float CompletionPercentage { get; set; } = 0;

    // Lesson có hoàn thành không (>= 90% modules completed)
    public bool IsCompleted { get; set; } = false;

    // % video đã xem (0-100)
    public float VideoProgressPercentage { get; set; } = 0;

    // Thời gian xem video cuối cùng (giây)
    public int LastWatchedPositionSeconds { get; set; } = 0;

    // Tổng thời gian học lesson (giây)
    public int TotalTimeSpentSeconds { get; set; } = 0;

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // ===== NEW MODULE TRACKING =====
    public int CompletedModules { get; set; } = 0;
    public int TotalModules { get; set; } = 0;


    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User User { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
    
    // ===== BUSINESS LOGIC =====
    public void UpdateModuleProgress(int totalModules, int completedModules)
    {
        TotalModules = totalModules;
        CompletedModules = completedModules;
        
        // Auto-calculate completion percentage based on modules
        CompletionPercentage = totalModules > 0 ? (float)completedModules / totalModules * 100 : 0;
        
        // Mark as completed if >= 90%
        var wasCompleted = IsCompleted;
        IsCompleted = CompletionPercentage >= 90f;
        
        // Set completion timestamp when first completed
        if (IsCompleted && !wasCompleted)
        {
            CompletedAt = DateTime.UtcNow;
            if (StartedAt == null) StartedAt = DateTime.UtcNow;
        }
        else if (!IsCompleted && wasCompleted)
        {
            CompletedAt = null; // Reset if dropped below 90%
        }
        
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void MarkVideoProgress(int positionSeconds, float videoPercentage)
    {
        LastWatchedPositionSeconds = positionSeconds;
        VideoProgressPercentage = videoPercentage;
        
        if (StartedAt == null) StartedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public string GetProgressDisplay()
    {
        return $"{CompletedModules}/{TotalModules} modules ({CompletionPercentage:F1}%)";
    }
}
