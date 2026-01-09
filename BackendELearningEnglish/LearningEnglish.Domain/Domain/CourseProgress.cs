namespace LearningEnglish.Domain.Entities;

// Simple Course Progress Tracking
public class CourseProgress
{
    public int CourseProgressId { get; set; }
    public int UserId { get; set; }
    public int CourseId { get; set; }

    // Simple progress tracking
    public int CompletedLessons { get; set; } = 0;
    public int TotalLessons { get; set; } = 0;
    public decimal ProgressPercentage { get; set; } = 0; // Auto-calculated: CompletedLessons/TotalLessons * 100

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; } // Set when ProgressPercentage >= 90%
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User User { get; set; } = null!;
    public Course Course { get; set; } = null!;

    // Business Logic
    public bool IsCompleted => ProgressPercentage >= 80m;

    public void UpdateProgress(int totalLessons, int completedLessons)
    {
        TotalLessons = totalLessons;
        CompletedLessons = completedLessons;
        ProgressPercentage = totalLessons > 0 ? (decimal)completedLessons / totalLessons * 100 : 0;

        if (IsCompleted && CompletedAt == null)
        {
            CompletedAt = DateTime.UtcNow;
        }
        else if (!IsCompleted && CompletedAt != null)
        {
            CompletedAt = null; // Reset if progress dropped below 80%
        }

        LastUpdated = DateTime.UtcNow;
    }

    public string GetProgressDisplay()
    {
        return $"{CompletedLessons}/{TotalLessons} ({ProgressPercentage:F1}%)";
    }
}
