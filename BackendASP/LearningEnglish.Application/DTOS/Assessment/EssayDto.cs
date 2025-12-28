namespace LearningEnglish.Application.DTOs
{

    // DTO hiển thị thông tin cho Essay

    public class EssayDto
    {
        public int EssayId { get; set; }
        public int AssessmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Audio attachment
        public string? AudioUrl { get; set; }
        public string? AudioType { get; set; }

        // Image attachment
        public string? ImageUrl { get; set; }
        public string? ImageType { get; set; }

        // Submission count (thay vì full list để tránh circular reference)
        public int TotalSubmissions { get; set; }
    }


    // DTO cho tạo Essay(bài kiểm tra tự luận) mới

    public class CreateEssayDto
    {
        public int AssessmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Audio attachment (temp key from MinIO)
        public string? AudioTempKey { get; set; }
        public string? AudioType { get; set; }

        // Image attachment (temp key from MinIO)
        public string? ImageTempKey { get; set; }
        public string? ImageType { get; set; }
    }


    // DTO cho cập nhật bài kiểm tra tự luận
    public class UpdateEssayDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }

        // Audio attachment (temp key from MinIO)
        public string? AudioTempKey { get; set; }
        public string? AudioType { get; set; }

        // Image attachment (temp key from MinIO)
        public string? ImageTempKey { get; set; }
        public string? ImageType { get; set; }
    }

    // DTO cho EssaySubmission - bài nộp của học sinh
    public class EssaySubmissionDto
    {
        public int SubmissionId { get; set; }
        public int EssayId { get; set; }
        public int UserId { get; set; }

        // Nội dung bài làm
        public string? TextContent { get; set; }

        // File đính kèm
        public string? AttachmentUrl { get; set; }
        public string? AttachmentType { get; set; }

        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; } = string.Empty;

        // � Grading Information (1 điểm duy nhất - AI hoặc Teacher ghi đè)
        // ═══════════════════════════════════════════════
        // AI GRADING (có thể null nếu chưa dùng AI)
        // ═══════════════════════════════════════════════
        public decimal? AiScore { get; set; }
        public string? AiFeedback { get; set; }
        public DateTime? AiGradedAt { get; set; }

        // ═══════════════════════════════════════════════
        // TEACHER/ADMIN GRADING (có thể null nếu chưa chấm)
        // ═══════════════════════════════════════════════
        public decimal? TeacherScore { get; set; }
        public string? TeacherFeedback { get; set; }
        public DateTime? TeacherGradedAt { get; set; }
        public int? GradedByTeacherId { get; set; }
        public string? GradedByTeacherName { get; set; }

        // ═══════════════════════════════════════════════
        // FINAL SCORE (ưu tiên Teacher, nếu không có thì AI)
        // ═══════════════════════════════════════════════
        public decimal? Score { get; set; }
        public string? Feedback { get; set; }
        public DateTime? GradedAt { get; set; }
        
        // Max score từ assessment
        public decimal? MaxScore { get; set; }

        // User info
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        
        // Essay info (để student review)
        public string? EssayTitle { get; set; }
        public string? EssayDescription { get; set; }
    }


    // DTO cho tạo submission mới
    public class CreateEssaySubmissionDto
    {
        public int EssayId { get; set; }
        public string? TextContent { get; set; }

        // File đính kèm (temp key từ MinIO)
        public string? AttachmentTempKey { get; set; }
        public string? AttachmentType { get; set; }
    }


    // DTO cho cập nhật submission

    public class UpdateEssaySubmissionDto
    {
        public string? TextContent { get; set; }

        // File đính kèm (temp key từ MinIO)
        public string? AttachmentTempKey { get; set; }
        public string? AttachmentType { get; set; }

        // Có xóa attachment cũ không
        public bool RemoveAttachment { get; set; } = false;
    }
}