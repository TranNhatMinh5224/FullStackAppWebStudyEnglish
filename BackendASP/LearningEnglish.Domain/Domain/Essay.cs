using LearningEnglish.Domain.Enums;
namespace LearningEnglish.Domain.Entities
{
    // Tạo Essay cho bài kiểm tra dạng tự luận
    public class Essay
    {
        public int EssayId { get; set; }
        public int AssessmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public AssessmentType Type { get; set; } = AssessmentType.Essay;
        
        // Navigation Properties
        public Assessment? Assessment { get; set; }
        public List<EssaySubmission> EssaySubmissions { get; set; } = new();
    }
}


