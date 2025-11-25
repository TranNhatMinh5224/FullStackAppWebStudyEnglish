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
        public float? DurationInSeconds { get; set; }
        
        // Scores from Azure
        public double AccuracyScore { get; set; }
        public double FluencyScore { get; set; }
        public double CompletenessScore { get; set; }
        public double PronunciationScore { get; set; }
        
        public string? RecognizedText { get; set; }
        public string? Feedback { get; set; }
        public string Status { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
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
        
        // MinIO temp key from upload
        public string AudioTempKey { get; set; } = string.Empty;
        public string? AudioType { get; set; }
        public long? AudioSize { get; set; }
        public float? DurationInSeconds { get; set; }
    }

    // DTO for listing pronunciation assessments (lighter version)
    public class ListPronunciationAssessmentDto
    {
        public int PronunciationAssessmentId { get; set; }
        public int UserId { get; set; }
        public int? FlashCardId { get; set; }
        public string ReferenceText { get; set; } = string.Empty;
        public double PronunciationScore { get; set; }
        public string? RecognizedText { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? FlashCardWord { get; set; }
    }
    
    // DTO for Azure Speech assessment result
    public class AzureSpeechAssessmentResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        
        public double AccuracyScore { get; set; }
        public double FluencyScore { get; set; }
        public double CompletenessScore { get; set; }
        public double PronunciationScore { get; set; }
        
        public string? RecognizedText { get; set; }
        public string? DetailedResultJson { get; set; }
        public string? RawResponse { get; set; }
    }
}


