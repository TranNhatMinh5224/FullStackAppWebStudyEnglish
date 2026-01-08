namespace LearningEnglish.Application.DTOs
{
    public class QuizSectionDto
    {
        public int QuizSectionId { get; set; }
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

    }
    public class CreateQuizSectionDto
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
    public class UpdateQuizSectionDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class ListQuizSectionDto
    {
        public int QuizSectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO cho bulk tạo Quiz Section với Groups và Questions
    /// </summary>
    public class QuizSectionBulkCreateDto
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>
        /// Danh sách QuizGroups kèm questions cho section này
        /// </summary>
        public List<QuizGroupBulkCreateDto> QuizGroups { get; set; } = new();

        /// <summary>
        /// Danh sách questions độc lập (không thuộc group nào)
        /// </summary>
        public List<QuestionCreateDto> StandaloneQuestions { get; set; } = new();
    }

    /// <summary>
    /// DTO cho bulk tạo QuizGroup kèm questions
    /// </summary>
    public class QuizGroupBulkCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Title { get; set; } = string.Empty;

        public float SumScore { get; set; }

        // Thứ tự hiển thị
        public int DisplayOrder { get; set; } = 0;

        // Media handling
        public string? ImgTempKey { get; set; }
        public string? ImgType { get; set; }

        public string? VideoTempKey { get; set; }
        public string? VideoType { get; set; }
        public int? VideoDuration { get; set; }

        /// <summary>
        /// Danh sách questions cho group này
        /// </summary>
        public List<QuestionCreateDto> Questions { get; set; } = new();
    }

}