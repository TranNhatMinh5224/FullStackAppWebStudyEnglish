using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.DTOs
{
    // DTO danh sách nộp bài luận
    public class EssaySubmissionListDto
    {
        public int SubmissionId { get; set; }
        public int UserId { get; set; }

        // Thông tin học sinh
        public string? UserName { get; set; }
        public string? UserAvatarUrl { get; set; } // Full URL to avatar

        // Thông tin submission cơ bản
        public DateTime SubmittedAt { get; set; }
        public SubmissionStatus Status { get; set; }

        // ═══════════════════════════════════════════════
        // AI GRADING (hiển thị riêng trong bảng)
        // ═══════════════════════════════════════════════
        public decimal? AiScore { get; set; }
        public DateTime? AiGradedAt { get; set; }

        // ═══════════════════════════════════════════════
        // TEACHER/ADMIN GRADING (hiển thị riêng trong bảng)
        // ═══════════════════════════════════════════════
        public decimal? TeacherScore { get; set; }
        public DateTime? TeacherGradedAt { get; set; }
        public int? GradedByTeacherId { get; set; }  // null = Admin chấm, có giá trị = Teacher chấm

        // ═══════════════════════════════════════════════
        // FINAL SCORE (để hiển thị cột điểm cuối cùng)
        // ═══════════════════════════════════════════════
        public decimal? Score { get; set; }  // FinalScore = TeacherScore ?? AiScore
        public DateTime? GradedAt { get; set; }  // TeacherGradedAt ?? AiGradedAt
        public string? FeedbackPreview { get; set; }  // 100 ký tự đầu (TeacherFeedback ?? Feedback)

        // Có file đính kèm không
        public bool HasAttachment { get; set; }
    }
}
