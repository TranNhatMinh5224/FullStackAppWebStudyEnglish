namespace LearningEnglish.Domain.Entities
{

    public class Notification
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User? User { get; set; }

        public string? Title { get; set; }

        public string? Message { get; set; }

        public NotificationType Type { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        // Optional: Link to related entity (course, lesson, etc.)
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }

        // For email notifications
        public bool IsEmailSent { get; set; } = false;
        public DateTime? EmailSentAt { get; set; }
    }

    public enum NotificationType
    {
        CourseEnrollment,
        CourseCompletion,
        LessonReminder,
        QuizDeadline,
        AssessmentGraded,
        PaymentSuccess,
        SystemAnnouncement,
        StudyStreak,
        Achievement,
        General
    }
}