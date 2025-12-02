namespace LearningEnglish.Domain.Entities
{
    /// <summary>
    /// Aggregated pronunciation progress - NO individual assessments stored
    /// Only summary metrics are persisted to minimize DB size
    /// </summary>
    public class PronunciationProgress
    {
        public int PronunciationProgressId { get; set; }
        public int UserId { get; set; }
        public int FlashCardId { get; set; }

        // Aggregated metrics
        public int TotalAttempts { get; set; } = 0;
        public double BestScore { get; set; } = 0;
        public DateTime? BestScoreDate { get; set; }
        public DateTime? LastPracticedAt { get; set; }
        
        // Average scores (rolling average of all attempts)
        public double AvgAccuracyScore { get; set; } = 0;
        public double AvgFluencyScore { get; set; } = 0;
        public double AvgCompletenessScore { get; set; } = 0;
        public double AvgPronunciationScore { get; set; } = 0;
        
        // Last attempt scores (for comparison)
        public double LastAccuracyScore { get; set; } = 0;
        public double LastFluencyScore { get; set; } = 0;
        public double LastPronunciationScore { get; set; } = 0;
        
        // Phoneme tracking (JSON: {"θ": 65.5, "ð": 58.2})
        public string? WeakPhonemesJson { get; set; }  // Phonemes with avg score < 70
        public string? StrongPhonemesJson { get; set; } // Phonemes with avg score >= 85
        
        // Practice streak
        public int ConsecutiveDaysStreak { get; set; } = 0;
        public DateTime? LastStreakDate { get; set; }

        // Mastery indicator
        public bool IsMastered { get; set; } = false; // BestScore >= 90 AND AvgScore >= 85
        public DateTime? MasteredAt { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public User User { get; set; } = null!;
        public FlashCard FlashCard { get; set; } = null!;

        /// <summary>
        /// Update progress after new realtime assessment (no persistence of assessment)
        /// </summary>
        public void UpdateAfterAssessment(
            double accuracyScore,
            double fluencyScore, 
            double completenessScore,
            double pronunciationScore,
            List<string> problemPhonemes,
            List<string> strongPhonemes,
            DateTime attemptTime)
        {
            TotalAttempts++;
            LastPracticedAt = attemptTime;
            
            // Update last scores
            LastAccuracyScore = accuracyScore;
            LastFluencyScore = fluencyScore;
            LastPronunciationScore = pronunciationScore;

            // Update rolling averages (weighted average)
            if (TotalAttempts == 1)
            {
                AvgAccuracyScore = accuracyScore;
                AvgFluencyScore = fluencyScore;
                AvgCompletenessScore = completenessScore;
                AvgPronunciationScore = pronunciationScore;
            }
            else
            {
                // Rolling average: newAvg = (oldAvg * (n-1) + newValue) / n
                AvgAccuracyScore = (AvgAccuracyScore * (TotalAttempts - 1) + accuracyScore) / TotalAttempts;
                AvgFluencyScore = (AvgFluencyScore * (TotalAttempts - 1) + fluencyScore) / TotalAttempts;
                AvgCompletenessScore = (AvgCompletenessScore * (TotalAttempts - 1) + completenessScore) / TotalAttempts;
                AvgPronunciationScore = (AvgPronunciationScore * (TotalAttempts - 1) + pronunciationScore) / TotalAttempts;
            }

            // Update best score
            if (pronunciationScore > BestScore)
            {
                BestScore = pronunciationScore;
                BestScoreDate = attemptTime;
            }
            
            // Update phoneme tracking (aggregate from all attempts)
            UpdatePhonemeTracking(problemPhonemes, strongPhonemes);
            
            // Update practice streak
            UpdatePracticeStreak(attemptTime);
            
            // Check mastery status
            if (BestScore >= 90 && AvgPronunciationScore >= 85 && !IsMastered)
            {
                IsMastered = true;
                MasteredAt = attemptTime;
            }

            UpdatedAt = DateTime.UtcNow;
        }
        
        private void UpdatePhonemeTracking(List<string> problemPhonemes, List<string> strongPhonemes)
        {
            // Simple JSON merge logic - you can enhance with proper scoring
            if (problemPhonemes.Any())
            {
                WeakPhonemesJson = System.Text.Json.JsonSerializer.Serialize(problemPhonemes.Distinct().ToList());
            }
            
            if (strongPhonemes.Any())
            {
                StrongPhonemesJson = System.Text.Json.JsonSerializer.Serialize(strongPhonemes.Distinct().ToList());
            }
        }
        
        private void UpdatePracticeStreak(DateTime attemptTime)
        {
            if (LastStreakDate == null)
            {
                ConsecutiveDaysStreak = 1;
                LastStreakDate = attemptTime.Date;
                return;
            }
            
            var daysSinceLastPractice = (attemptTime.Date - LastStreakDate.Value.Date).Days;
            
            if (daysSinceLastPractice == 1)
            {
                // Consecutive day
                ConsecutiveDaysStreak++;
            }
            else if (daysSinceLastPractice > 1)
            {
                // Streak broken
                ConsecutiveDaysStreak = 1;
            }
            // Same day = no change
            
            LastStreakDate = attemptTime.Date;
        }
    }
}
