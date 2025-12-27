using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.DTOs
{
    // DTO danh sách nộp bài luận
    public class EssaySubmissionListDto
    {
        public int SubmissionId { get; set; }
        public int EssayId { get; set; }
        public int UserId { get; set; }

        // Thông tin học sinh
        public string? UserName { get; set; }
        public string? UserAvatarUrl { get; set; } // Full URL to avatar

        // Thông tin submission cơ bản
        public DateTime SubmittedAt { get; set; }
        public SubmissionStatus Status { get; set; }

        // Thông tin chấm điểm
        public decimal? Score { get; set; }
        public DateTime? GradedAt { get; set; }
        public int? GradedByTeacherId { get; set; }  // null = AI chấm, có giá trị = Teacher chấm
        public string? FeedbackPreview { get; set; }  // 100 ký tự đầu

        // Có file đính kèm không
        public bool HasAttachment { get; set; }
    }
}
