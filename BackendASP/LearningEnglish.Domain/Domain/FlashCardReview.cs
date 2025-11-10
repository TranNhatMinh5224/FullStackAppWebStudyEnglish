namespace LearningEnglish.Domain.Entities;

// Lịch sử ôn tập từ vựng đơn giản
public class FlashCardReview
{
    public int FlashCardReviewId { get; set; }
    public int UserId { get; set; }
    public int FlashCardId { get; set; }

    // Điểm đánh giá (0-5): 0=Quên hoàn toàn, 5=Nhớ hoàn hảo
    public int Quality { get; set; } = 0;

    // Spaced Repetition Algorithm
    public float EasinessFactor { get; set; } = 2.5f;    // Độ dễ nhớ (1.3-2.5)
    public int IntervalDays { get; set; } = 1;           // Khoảng cách ôn tập (ngày)
    public int RepetitionCount { get; set; } = 0;        // Số lần ôn thành công liên tiếp

    public DateTime NextReviewDate { get; set; }
    public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User User { get; set; } = null!;
    public FlashCard FlashCard { get; set; } = null!;
}
