namespace CleanDemo.Domain.Entities;

// Streak - Chuỗi ngày học liên tục
public class Streak
{
    public int StreakId { get; set; }
    public int UserId { get; set; }

    // Chuỗi ngày hiện tại
    public int CurrentStreak { get; set; } = 0;

    // Chuỗi ngày dài nhất
    public int LongestStreak { get; set; } = 0;

    public DateTime? LastActivityDate { get; set; }

    // Ngày bắt đầu streak hiện tại
    public DateTime? CurrentStreakStartDate { get; set; }

    // Có freeze streak không (dùng 1 lần bỏ qua 1 ngày)
    public int FreezeCount { get; set; } = 0;

    public DateTime? LastFreezeUsedDate { get; set; }

    // Tổng số ngày đã học (không cần liên tục)
    public int TotalActiveDays { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User User { get; set; } = null!;
}
