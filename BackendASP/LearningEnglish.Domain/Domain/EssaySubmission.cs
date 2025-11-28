using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Domain.Entities;

// Bài nộp của học sinh cho Essay
public class EssaySubmission
{
    public int SubmissionId { get; set; }
    public int EssayId { get; set; }
    public int UserId { get; set; }

    // Nội dung bài làm
    public string? TextContent { get; set; }
    
    // File đính kèm (PDF, DOCX, etc.)
    public string? AttachmentKey { get; set; }
    public string? AttachmentType { get; set; }
    
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;

    // Navigation properties
    public User User { get; set; } = null!;
    public Essay Essay { get; set; } = null!;
}
