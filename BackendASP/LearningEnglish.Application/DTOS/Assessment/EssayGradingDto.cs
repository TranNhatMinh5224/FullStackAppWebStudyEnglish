namespace LearningEnglish.Application.DTOs;
public class EssayGradingResultDto
{
    public int SubmissionId { get; set; }
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public GradingBreakdown? Breakdown { get; set; }
    public List<string> Strengths { get; set; } = new();
    public List<string> Improvements { get; set; } = new();
    public DateTime GradedAt { get; set; }
    public bool GradedByTeacher { get; set; }
    public decimal? FinalScore { get; set; }
}

public class GradingBreakdown
{
    public decimal ContentScore { get; set; }
    public decimal LanguageScore { get; set; }
    public decimal OrganizationScore { get; set; }
    public decimal MechanicsScore { get; set; }
}

public class TeacherGradingDto
{
    public decimal Score { get; set; }
    public string Feedback { get; set; } = string.Empty;
}

// Internal DTO for parsing Gemini AI response
public class AiGradingResult
{
    public decimal Score { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public GradingBreakdown? Breakdown { get; set; }
    public List<string> Strengths { get; set; } = new();
    public List<string> Improvements { get; set; } = new();
}
