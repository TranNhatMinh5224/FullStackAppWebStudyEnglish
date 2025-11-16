namespace LearningEnglish.Application.DTOs;

// DTO cho vocabulary review
public class VocabularyReviewDto
{
    public int ReviewId { get; set; }
    public int FlashCardId { get; set; }
    public FlashCardDto FlashCard { get; set; } = null!;
    public int Quality { get; set; }
    public float EasinessFactor { get; set; }
    public int IntervalDays { get; set; }
    public int RepetitionCount { get; set; }
    public DateTime NextReviewDate { get; set; }
    public DateTime ReviewedAt { get; set; }
    public string ReviewStatus { get; set; } = string.Empty; // "New", "Learning", "Reviewing", "Mastered"
}

// DTO cho kết quả submit review
public class VocabularyReviewResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime NextReviewDate { get; set; }
    public int NewIntervalDays { get; set; }
    public float NewEasinessFactor { get; set; }
    public string ReviewStatus { get; set; } = string.Empty;
}

// DTO cho thống kê vocabulary
public class VocabularyStatsDto
{
    public int TotalCards { get; set; }
    public int DueToday { get; set; }
    public int MasteredCards { get; set; }
    public int LearningCards { get; set; }
    public int NewCards { get; set; }
    public int TotalReviews { get; set; }
    public double AverageQuality { get; set; }
    public int StreakDays { get; set; }
    public DateTime? LastReviewDate { get; set; }
}

// DTO cho submit review request
public class SubmitReviewRequestDto
{
    public int Quality { get; set; } // 0-5: 0=Quên hoàn toàn, 5=Nhớ hoàn hảo
}
