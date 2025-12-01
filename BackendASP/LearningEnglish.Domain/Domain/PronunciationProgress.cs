namespace LearningEnglish.Domain.Entities
{
    /// <summary>
    /// Simple best score tracking for each User-FlashCard pair
    /// Stores only the best pronunciation score
    /// </summary>
    public class PronunciationProgress
    {
        public int PronunciationProgressId { get; set; }
        public int UserId { get; set; }
        public int FlashCardId { get; set; }

        // Simple tracking
        public int TotalAttempts { get; set; } = 0;
        public double BestScore { get; set; } = 0;
        public int? BestAssessmentId { get; set; }  // Reference to best PronunciationAssessment
        public DateTime? BestScoreDate { get; set; }
        public DateTime? LastPracticedAt { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public User User { get; set; } = null!;
        public FlashCard FlashCard { get; set; } = null!;
        public PronunciationAssessment? BestAssessment { get; set; }

        // Business Logic - SIMPLE VERSION
        public void UpdateAfterNewAssessment(PronunciationAssessment assessment)
        {
            TotalAttempts++;
            LastPracticedAt = assessment.CreatedAt;

            // Update best score if new score is better
            if (assessment.PronunciationScore > BestScore)
            {
                BestScore = assessment.PronunciationScore;
                BestScoreDate = assessment.CreatedAt;
                BestAssessmentId = assessment.PronunciationAssessmentId;
            }

            UpdatedAt = DateTime.UtcNow;
        }
    }
}
