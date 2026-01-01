
namespace LearningEnglish.Domain.Entities
{
    // Tạo Essay cho bài kiểm tra dạng tự luận
    public class Essay
    {
        public int EssayId { get; set; }
        public int AssessmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Audio attachment for essay question
        public string? AudioKey { get; set; }
        public string? AudioType { get; set; }

        // Image attachment for essay question
        
        public string? ImageKey { get; set; }
        public string? ImageType { get; set; }

     

        public decimal TotalPoints { get; set; } // Điểm tối đa của bài essay

        // Navigation Properties
        public Assessment? Assessment { get; set; }
        public List<EssaySubmission> EssaySubmissions { get; set; } = new();
    }
}


