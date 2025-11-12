namespace LearningEnglish.Application.DTOs
{
    public class QuizGroupDto
    {
        public int QuizGroupId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int QuizSectionId { get; set; }
        public float SumScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation Properties
        public QuizSectionDto? QuizSection { get; set; }
        public List<QuestionDto> Questions { get; set; } = new();
        public List<MediaAssetDto> QuizQuestionGroupMedias { get; set; } = new();
    }

    public class CreateQuizGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int QuizSectionId { get; set; }
        public float SumScore { get; set; }
    }

    public class UpdateQuizGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public float SumScore { get; set; }
    }

    public class ListQuizGroupDto
    {
        public int QuizGroupId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public float SumScore { get; set; }
        public int QuestionCount { get; set; }
    }

    // Temporary DTOs cho MediaAsset và Question (nếu chưa có)
    public class MediaAssetDto
    {
        public int MediaAssetId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeInBytes { get; set; }
    }

    public class QuestionDto
    {
        public int QuestionId { get; set; }
        public string StemText { get; set; } = string.Empty;
        public string? StemHtml { get; set; }
        public int Points { get; set; }
    }
}
