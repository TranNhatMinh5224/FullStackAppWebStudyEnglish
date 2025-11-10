namespace CleanDemo.Domain.Entities;

// Đánh giá phát âm đơn giản
public class PronunciationAssessment
{
    public int PronunciationAssessmentId { get; set; }
    public int UserId { get; set; }
    public int? FlashCardId { get; set; }
    public int? AssignmentId { get; set; }      

    // Văn bản cần đọc
    public string ReferenceText { get; set; } = string.Empty;

    // URL file audio ghi âm của user
    public string AudioUrl { get; set; } = string.Empty;

    // Điểm số tổng thể (0-100)
    public float OverallScore { get; set; } = 0;

    // Feedback đơn giản
    public string? Feedback { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User User { get; set; } = null!;
    public FlashCard? FlashCard { get; set; }
    public Assessment? Assignment { get; set; }
}
