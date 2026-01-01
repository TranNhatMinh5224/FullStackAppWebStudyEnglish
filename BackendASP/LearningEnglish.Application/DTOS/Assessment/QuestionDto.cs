using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.DTOs
{

    public class QuestionCreateDto
    {
        // Thông tin cơ bản
        public QuestionType Type { get; set; } = QuestionType.MultipleChoice;
        public string StemText { get; set; } = string.Empty;
        public string? StemHtml { get; set; }

        // Liên kết
        public int? QuizGroupId { get; set; }
        public int? QuizSectionId { get; set; }

        // Điểm & chiến lược chấm
        public decimal Points { get; set; } = 10m;
        public string? CorrectAnswersJson { get; set; }

        public string MetadataJson { get; set; } = "{}";

        public string? Explanation { get; set; }

        // Media handling
        public string? MediaType { get; set; }
        public string? MediaTempKey { get; set; } // MinIO temp key for file upload

        public List<AnswerOptionCreateDto> Options { get; set; } = new();
    }

    public class AnswerOptionCreateDto
    {
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public string? Feedback { get; set; }

        // Media handling
        public string? MediaType { get; set; }
        public string? MediaTempKey { get; set; } // MinIO temp key for file upload
    }

    // DTO LẤY DỮ LIỆU 

    public class QuestionReadDto
    {
        public int QuestionId { get; set; }
        public QuestionType Type { get; set; }
        public string StemText { get; set; } = string.Empty;
        public string? StemHtml { get; set; }

        public int? QuizGroupId { get; set; }
        public int? QuizSectionId { get; set; }

        public decimal Points { get; set; }
        public string? CorrectAnswersJson { get; set; }
        public string MetadataJson { get; set; } = "{}";

        public string? Explanation { get; set; }
        public string? MediaUrl { get; set; }
        public string? MediaType { get; set; }

        public List<AnswerOptionReadDto> Options { get; set; } = new();

        // Bổ sung theo entity
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AnswerOptionReadDto
    {
        public int AnswerOptionId { get; set; }
        public int QuestionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public string? MediaUrl { get; set; }
        public string? MediaType { get; set; }
        public string? Feedback { get; set; }
    }
    // DTO CẬP NHẬT
    public class QuestionUpdateDto : QuestionCreateDto
    {
    }
    // Bulk DTOs
    public class QuestionBulkCreateDto
    {

        public List<QuestionCreateDto> Questions { get; set; } = new();
    }
    public class QuestionBulkResponseDto
    {
        public List<int> CreatedQuestionIds { get; set; } = new();
    }
    public class BulkItemResult
    {
        public int QuestionId { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
    public class QuestionBulkDetailedResponseDto
    {
        public List<BulkItemResult> Results { get; set; } = new();
    }

}
