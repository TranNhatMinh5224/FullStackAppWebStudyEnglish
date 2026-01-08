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
    public string? Feedback { get; set; }
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
// DTO for batch grading result (Teacher batch grade all submissions)
public class BatchGradingResultDto
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int FailCount { get; set; }
    public List<GradingResult> Results { get; set; } = new();
}

public class GradingResult
{
    public int SubmissionId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
}

// DTO for essay statistics (Teacher view)
public class EssayStatisticsDto
{
    public int EssayId { get; set; }
    public int TotalSubmissions { get; set; }
    public int Pending { get; set; }          // Chưa chấm
    public int GradedByAi { get; set; }       // AI đã chấm
    public int GradedByTeacher { get; set; }  // Teacher override
    public int NoTextContent { get; set; }    // Chỉ có file, không chấm được AI
}