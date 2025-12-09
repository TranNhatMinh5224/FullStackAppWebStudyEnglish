using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Domain.Entities;

// Nhắc nhở học tập (Study Reminders) - push notifications, email
public class StudyReminder
{
    public int StudyReminderId { get; set; }
    public int UserId { get; set; }

    // Loại reminder - Now using enum instead of string
    public ReminderType Type { get; set; } = ReminderType.DailyStudy;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    // Thời gian nhắc (giờ:phút) - Format: "14:30"
    public string ScheduledTime { get; set; } = "19:00";

    // Ngày trong tuần - Now using enum flags instead of JSON string
    public DaysOfWeek DaysOfWeek { get; set; } = DaysOfWeek.Weekdays;

    public string? TimeZone { get; set; }

    // Có gửi push notification không
    public bool IsPushEnabled { get; set; } = true;

    // Có gửi email không
    public bool IsEmailEnabled { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public DateTime? LastSentAt { get; set; }
    public DateTime? NextScheduledAt { get; set; }

    // Số lần đã gửi
    public int SentCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User User { get; set; } = null!;
}
