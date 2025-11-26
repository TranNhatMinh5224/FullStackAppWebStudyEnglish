using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public bool IsEmailSent { get; set; }
        public DateTime? EmailSentAt { get; set; }
    }

    public class CreateNotificationDto
    {
        public required int UserId { get; set; }
        public required string Title { get; set; }
        public required string Message { get; set; }
        public required NotificationType Type { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public bool SendEmail { get; set; } = false;
    }

    public class StudyReminderDto
    {
        public int StudyReminderId { get; set; }
        public int UserId { get; set; }
        public ReminderType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ScheduledTime { get; set; } = string.Empty;
        public DaysOfWeek DaysOfWeek { get; set; }
        public string? TimeZone { get; set; }
        public bool IsPushEnabled { get; set; }
        public bool IsEmailEnabled { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastSentAt { get; set; }
        public DateTime? NextScheduledAt { get; set; }
        public int SentCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateStudyReminderDto
    {
        public required int UserId { get; set; }
        public required ReminderType Type { get; set; }
        public required string Title { get; set; }
        public required string Message { get; set; }
        public required string ScheduledTime { get; set; } = "19:00";
        public DaysOfWeek DaysOfWeek { get; set; } = DaysOfWeek.Weekdays;
        public string? TimeZone { get; set; }
        public bool IsPushEnabled { get; set; } = true;
        public bool IsEmailEnabled { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}