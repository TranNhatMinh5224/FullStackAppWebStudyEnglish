namespace LearningEnglish.Application.DTOs
{
    // DTO for PronunciationAssessment realtime response (not stored in DB)
    public class PronunciationAssessmentDto
    {
        public int UserId { get; set; }
        public int? FlashCardId { get; set; }
        public int? AssessmentId { get; set; }
        
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
        
        // 🆕 Word-level detailed feedback
        public List<WordPronunciationDetail> Words { get; set; } = new();
        public List<string> ProblemPhonemes { get; set; } = new();
        public List<string> StrongPhonemes { get; set; } = new();
    }

    // DTO for creating new pronunciation assessment
    public class CreatePronunciationAssessmentDto
    {
        // FlashCard ID is REQUIRED - referenceText will be fetched from FlashCard.Word
        public int FlashCardId { get; set; }
        
        
        public string AudioTempKey { get; set; } = string.Empty;
        
        // Optional fields
        public string? AudioType { get; set; }
        public long? AudioSize { get; set; }
        public float? DurationInSeconds { get; set; }
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
        
        // 🆕 Word-level details
        public List<WordPronunciationDetail> Words { get; set; } = new();
        
        // 🆕 Phoneme analysis
        public List<string> ProblemPhonemes { get; set; } = new();
        public List<string> StrongPhonemes { get; set; } = new();
    }

    // 🆕 DTO for word-level pronunciation details
    public class WordPronunciationDetail
    {
        public string Word { get; set; } = string.Empty;
        public double AccuracyScore { get; set; }
        public string ErrorType { get; set; } = "None"; // None, Mispronunciation, Omission, Insertion
        public int Offset { get; set; }        // Position in audio (ticks: 10,000 ticks = 1ms)
        public int Duration { get; set; }      // Duration (ticks)
        public List<PhonemeDetail> Phonemes { get; set; } = new();
    }

    // 🆕 DTO for phoneme-level details
    public class PhonemeDetail
    {
        public string Phoneme { get; set; } = string.Empty;           // IPA symbol: "θ", "ɜr"
        public string PhonemeDisplay { get; set; } = string.Empty;    // User-friendly: "th", "er"
        public double AccuracyScore { get; set; }
        public int Offset { get; set; }
        public int Duration { get; set; }
    }
}


