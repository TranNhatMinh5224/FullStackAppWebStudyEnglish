namespace LearningEnglish.Domain.Entities
{
    public class PronunciationAssessment
    {
        public int PronunciationAssessmentId { get; set; }
        public int UserId { get; set; }
        public int? FlashCardId { get; set; }
        public int? AssessmentId { get; set; }

        // Input
        public string ReferenceText { get; set; } = string.Empty;
        public string AudioKey { get; set; } = string.Empty;
        public string? AudioType { get; set; }
        public long? AudioSize { get; set; }
        public float? DurationInSeconds { get; set; }

        // Output Scores
        public double AccuracyScore { get; set; } = 0;
        public double FluencyScore { get; set; } = 0;
        public double CompletenessScore { get; set; } = 0;
        public double PronunciationScore { get; set; } = 0;

        public string? RecognizedText { get; set; }

        // 🆕 Store word-level and phoneme analysis as JSON
        public string? WordsDataJson { get; set; }          // Serialized WordPronunciationDetail[]
        public string? ProblemPhonemesJson { get; set; }    // Serialized string[] of problem phonemes
        public string? StrongPhonemesJson { get; set; }     // Serialized string[] of strong phonemes
     
        public string? DetailedResultJson { get; set; }


        public string? AzureRawResponse { get; set; }

    
        public string? Feedback { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public AssessmentStatus Status { get; set; } = AssessmentStatus.Pending;

        public User User { get; set; } = null!;
        public FlashCard? FlashCard { get; set; }
        public Assessment? Assessment { get; set; }
    }

    public enum AssessmentStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3
    }
}
