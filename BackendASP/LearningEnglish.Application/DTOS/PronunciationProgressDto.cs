namespace LearningEnglish.Application.DTOs
{
    // DTO for PronunciationProgress response (simplified)
    public class PronunciationProgressDto
    {
        public int PronunciationProgressId { get; set; }
        public int UserId { get; set; }
        public int FlashCardId { get; set; }

        // FlashCard info
        public string Word { get; set; } = string.Empty;
        public string? Pronunciation { get; set; }
        public string? ImageUrl { get; set; }
        public string? AudioUrl { get; set; }

        // Simple statistics
        public double BestScore { get; set; }
        public int TotalAttempts { get; set; }
        public DateTime? LastPracticedAt { get; set; }

        // Best assessment reference
        public int? BestAssessmentId { get; set; }
    }

    // DTO for flashcard with pronunciation progress (for module view)
    public class FlashCardWithPronunciationProgressDto
    {
        public int FlashCardId { get; set; }
        public string Word { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public string? Pronunciation { get; set; }
        public string? AudioUrl { get; set; }
        public string? ImageUrl { get; set; }

        // Progress info
        public int TotalAttempts { get; set; }
        public double BestScore { get; set; }
        public DateTime? LastPracticedAt { get; set; }

        // Status flags
        public bool HasPracticed => TotalAttempts > 0;
        public bool IsGoodScore => BestScore >= 80;
        public bool NeedsPractice => TotalAttempts == 0 || BestScore < 70;
    }
}
