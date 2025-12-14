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

        public string Type { get; set; } = "Essay";

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

        // User info (không include full UserDto để tránh circular reference)
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
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
    }
}