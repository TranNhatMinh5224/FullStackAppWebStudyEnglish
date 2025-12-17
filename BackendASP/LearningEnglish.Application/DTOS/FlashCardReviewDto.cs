namespace LearningEnglish.Application.DTOs
{
    // DTO ôn tập lại thẻ
    public class ReviewFlashCardDto
    {
        public int FlashCardId { get; set; }


        /// Quality rating (0-5):
        /// 0 = Quên hoàn toàn 
        /// 1 = Sai hoàn toàn (Incorrect response)
        /// 2 = Sai nhưng nhớ khi xem đáp án (Incorrect but remembered upon seeing answer)
        /// 3 = Đúng nhưng khó khăn (Correct with difficulty)
        /// 4 = Đúng với chút do dự (Correct with some hesitation)
        /// 5 = Đúng và dễ dàng (Perfect response)

        public int Quality { get; set; }
    }

    // phản hồi sau khi ôn tập thẻ
    public class ReviewFlashCardResponseDto
    {
        public int FlashCardReviewId { get; set; }
        public int FlashCardId { get; set; }
        public string Word { get; set; } = string.Empty;
        public int Quality { get; set; }

        // khoảng lặp tiếp theo 
        public float EasinessFactor { get; set; }
        public int IntervalDays { get; set; }
        public int RepetitionCount { get; set; }
        public DateTime NextReviewDate { get; set; }

        public string Message { get; set; } = string.Empty;
    }

    // DTO thẻ cần dược ôn tập
    public class DueFlashCardDto
    {
        public int FlashCardId { get; set; }
        public int? ModuleId { get; set; }
        public string Word { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public string? Pronunciation { get; set; }
        public string? ImageUrl { get; set; }
        public string? AudioUrl { get; set; }
        public string? PartOfSpeech { get; set; }
        public string? Example { get; set; }
        public string? ExampleTranslation { get; set; }

        // ngày lặp tiếp theo
        public DateTime NextReviewDate { get; set; }
        public int IntervalDays { get; set; }
        public int RepetitionCount { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysOverdue { get; set; }
    }

    // phản hồi ngày đến hạn
    public class DueFlashCardsResponseDto
    {
        public int TotalDue { get; set; }
        public int NewCards { get; set; } // thẻ chua on
        public int ReviewCards { get; set; } // the da on 1 lan
        public int OverdueCards { get; set; }
        public List<DueFlashCardDto> FlashCards { get; set; } = new();
    }

    // DTO for review statistics dashboard
    public class ReviewStatisticsDto
    {
        public int TotalCards { get; set; } // Total flashcards user has reviewed
        public int DueToday { get; set; }
        public int NewToday { get; set; }
        public int ReviewedToday { get; set; }
        public int MasteredCards { get; set; } // IntervalDays >= 21 (3 weeks)

        // Weekly stats
        public int ReviewedThisWeek { get; set; }
        public int NewThisWeek { get; set; }

        // Performance
        public decimal AverageQuality { get; set; } // Average quality score (0-5)
        public decimal SuccessRate { get; set; } // % of reviews with quality >= 3

        // Upcoming reviews
        public Dictionary<string, int> UpcomingReviews { get; set; } = new(); // Date -> Count

        // Streak
        public int CurrentStreak { get; set; } // Days in a row with reviews
        public int LongestStreak { get; set; }
    }

    // DTO for resetting a flashcard review progress
    public class ResetFlashCardReviewDto
    {
        public int FlashCardId { get; set; }
    }
}
