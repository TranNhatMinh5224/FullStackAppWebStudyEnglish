namespace LearningEnglish.Application.DTOs
{
  
    // DTO hiển thị thông tin cho Essay
    
    public class EssayDto
    {
        public int EssayId { get; set; }
        public int AssessmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = "Essay";
        
        // Navigation properties
        public AssessmentDto? Assessment { get; set; }
        public List<EssaySubmissionDto>? EssaySubmissions { get; set; }
    }

    
    // DTO cho tạo Essay(bài kiểm tra tự luận) mới
  
    public class CreateEssayDto
    {
        public int AssessmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    
    // DTO cho cập nhật bài kiểm tra tự luận
        public class UpdateEssayDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    // DTO cho EssaySubmission - bài nộp của học sinh
    public class EssaySubmissionDto
    {
        public int SubmissionId { get; set; }
        public int EssayId { get; set; }
        public int UserId { get; set; }
        
        // Nội dung bài làm
        public string? TextContent { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        
        // Navigation properties
        public UserDto? User { get; set; }
        public EssayDto? Essay { get; set; }
    }


    // DTO cho tạo submission mới
    public class CreateEssaySubmissionDto
    {
        public int EssayId { get; set; }
        public string? TextContent { get; set; }
    }

    
    // DTO cho cập nhật submission
    
    public class UpdateEssaySubmissionDto
    {
        public string? TextContent { get; set; }
    }
}