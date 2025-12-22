namespace LearningEnglish.Application.DTOs
{
    public class AssessmentDto
    {
        public int AssessmentId { get; set; }
        public int ModuleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? OpenAt { get; set; }
        public DateTime? DueAt { get; set; }


        public string? TimeLimit { get; set; }

        public bool IsPublished { get; set; } = false;
        public decimal TotalPoints { get; set; }
        public int PassingScore { get; set; }

        // Navigation DTOs
        public string? ModuleTitle { get; set; }
    }

    public class CreateAssessmentDto
    {
        // Assessment thuộc về Module (Quiz/Assignment type)
        public int ModuleId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public DateTime? OpenAt { get; set; }
        public DateTime? DueAt { get; set; }

        // Time limit in format "HH:MM:SS" (e.g., "01:30:00" for 1 hour 30 minutes)
        public string? TimeLimit { get; set; }

        public bool IsPublished { get; set; } = false;
        public decimal TotalPoints { get; set; }
        public int PassingScore { get; set; }
    }
    public class UpdateAssessmentDto : CreateAssessmentDto
    {
    }
}
