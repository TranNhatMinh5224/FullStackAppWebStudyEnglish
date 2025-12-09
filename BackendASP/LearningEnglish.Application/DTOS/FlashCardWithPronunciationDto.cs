namespace LearningEnglish.Application.DTOs
{
    /// <summary>
    /// FlashCard with pronunciation progress for practice view
    /// </summary>
    public class FlashCardWithPronunciationDto
    {
        // FlashCard info
        public int FlashCardId { get; set; }
        public string Word { get; set; } = string.Empty;
        public string Definition { get; set; } = string.Empty;
        public string? Example { get; set; }
        public string? ImageUrl { get; set; }
        public string? AudioUrl { get; set; }
        public string? Phonetic { get; set; }

        // Pronunciation Progress (null if never practiced)
        public PronunciationProgressSummary? Progress { get; set; }
    }

    public class PronunciationProgressSummary
    {
        public int TotalAttempts { get; set; }
        public double BestScore { get; set; }
        public DateTime? BestScoreDate { get; set; }
        public DateTime? LastPracticedAt { get; set; }

        public double AvgPronunciationScore { get; set; }
        public double LastPronunciationScore { get; set; }

        public int ConsecutiveDaysStreak { get; set; }
        public bool IsMastered { get; set; }
        public DateTime? MasteredAt { get; set; }

        // Status indicators
        public string Status { get; set; } = "Not Started"; // Not Started, Practicing, Mastered
        public string StatusColor { get; set; } = "gray"; // gray, yellow, green
    }
}
