namespace LearningEnglish.Application.DTOs
{
    // DTO for PronunciationAssessment response
    public class PronunciationAssessmentDto
    {
        public int PronunciationAssessmentId { get; set; }
        public int UserId { get; set; }
        public int? FlashCardId { get; set; }
        public int? AssignmentId { get; set; }
        public string ReferenceText { get; set; } = string.Empty;
        public string AudioUrl { get; set; } = string.Empty;
        public string? AudioType { get; set; }
        public long? AudioSize { get; set; }
        public float OverallScore { get; set; }
        public string? Feedback { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation info
        public string? UserName { get; set; }
        public string? FlashCardWord { get; set; }
    }

    // DTO for creating new pronunciation assessment
    public class CreatePronunciationAssessmentDto
    {
        public int? FlashCardId { get; set; }
        public int? AssignmentId { get; set; }
        public string ReferenceText { get; set; } = string.Empty;
        public string AudioUrl { get; set; } = string.Empty;
        public string? AudioType { get; set; }
        public long? AudioSize { get; set; }
    }

    // DTO for updating pronunciation assessment
    public class UpdatePronunciationAssessmentDto
    {
        public string? ReferenceText { get; set; }
        public string? AudioUrl { get; set; }
        public string? AudioType { get; set; }
        public long? AudioSize { get; set; }
        public float? OverallScore { get; set; }
        public string? Feedback { get; set; }
    }

    // DTO for listing pronunciation assessments (lighter version)
    public class ListPronunciationAssessmentDto
    {
        public int PronunciationAssessmentId { get; set; }
        public int UserId { get; set; }
        public int? FlashCardId { get; set; }
        public string ReferenceText { get; set; } = string.Empty;
        public float OverallScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? FlashCardWord { get; set; }
    }
}

