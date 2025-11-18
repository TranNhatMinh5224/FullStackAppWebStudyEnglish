namespace LearningEnglish.Application.DTOs
{
    public class QuizGroupDto
    {
        public int QuizGroupId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int QuizSectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ImgUrl { get; set; }
        public string? VideoUrl { get; set; }
        
        public string? ImgType { get; set; }
        public string? VideoType { get; set; }
        public int? VideoDuration { get; set; }
        
        public float SumScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation Properties
        public QuizSectionDto? QuizSection { get; set; }
        public List<QuestionDto> Questions { get; set; } = new();
    }

    public class CreateQuizGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int QuizSectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ImgUrl { get; set; }
        public string? VideoUrl { get; set; }
        
        public string? ImgType { get; set; }
        public string? VideoType { get; set; }
        public int? VideoDuration { get; set; }
        
        public float SumScore { get; set; }
    }

    public class UpdateQuizGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ImgUrl { get; set; }
        public string? VideoUrl { get; set; }
        
        public string? ImgType { get; set; }
        public string? VideoType { get; set; }
        public int? VideoDuration { get; set; }
        
        public float SumScore { get; set; }
    }

    public class ListQuizGroupDto
    {
        public int QuizGroupId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ImgUrl { get; set; }
        public string? VideoUrl { get; set; }
        
        public string? ImgType { get; set; }
        public string? VideoType { get; set; }
        public int? VideoDuration { get; set; }
        
        public float SumScore { get; set; }
        public int QuestionCount { get; set; }
    }
}
