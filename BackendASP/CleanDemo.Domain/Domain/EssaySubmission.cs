using CleanDemo.Domain.Enums;

namespace CleanDemo.Domain.Entities;

// Bài nộp của học sinh cho Essay
public class EssaySubmission
{
    public int SubmissionId { get; set; }
    public int AssessmentId { get; set; }
    public int UserId { get; set; }

    // Nội dung bài làm
    public string? TextContent { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public StatusSubmission Status { get; set; } = StatusSubmission.Submitted;


    // Chấm điểm
    public decimal? Score { get; set; }
    public decimal? MaxScore { get; set; }
    public decimal? Percentage { get; set; }


    // Feedback từ teacher
    public int? GraderId { get; set; }
    public string? TeacherFeedback { get; set; }
    public string? PrivateNotes { get; set; }


    public User User { get; set; } = null!;
    public User? Grader { get; set; } // Teacher chấm bài
    public Assessment Assessment { get; set; } = null!;



}
