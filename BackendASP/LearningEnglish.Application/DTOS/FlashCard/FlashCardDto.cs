namespace LearningEnglish.Application.DTOs
{
    // DTO cho flashcard
    public class FlashCardDto
    {
        public int FlashCardId { get; set; }
        public int? ModuleId { get; set; }
        public string Word { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public string? Pronunciation { get; set; }
        public string? ImageUrl { get; set; }
        public string? AudioUrl { get; set; }

        public string? ImageType { get; set; }
        public string? AudioType { get; set; }

        // Thông tin bổ sung
        public string? PartOfSpeech { get; set; }
        public string? Example { get; set; }
        public string? ExampleTranslation { get; set; }
        public string? Synonyms { get; set; }
        public string? Antonyms { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // chuyển hướng module
        public string? ModuleName { get; set; }

        // thống kê SRS
        public int ReviewCount { get; set; }
        public decimal SuccessRate { get; set; }
        public DateTime? LastReviewedAt { get; set; }
        public DateTime? NextReviewAt { get; set; }
        public int CurrentLevel { get; set; } // SRS level (0-6)
    }

    // dto danh sách flashcard
    public class ListFlashCardDto
    {
        public int FlashCardId { get; set; }
        public int? ModuleId { get; set; }
        public string Word { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public string? Pronunciation { get; set; }
        public string? ImageUrl { get; set; }
        public string? AudioUrl { get; set; }
        public string? PartOfSpeech { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ReviewCount { get; set; }
        public decimal SuccessRate { get; set; }
        public int CurrentLevel { get; set; }
    }

    // DTO tạo 1 thẻ mới
    public class CreateFlashCardDto
    {
        public int? ModuleId { get; set; }
        public string Word { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public string? Pronunciation { get; set; }

        public string? PartOfSpeech { get; set; }
        public string? Example { get; set; }
        public string? ExampleTranslation { get; set; }
        public string? Synonyms { get; set; }
        public string? Antonyms { get; set; }

        public string? ImageTempKey { get; set; }
        public string? AudioTempKey { get; set; }
        public string? ImageType { get; set; }
        public string? AudioType { get; set; }
    }

    // DTO sửa flash card 
    public class UpdateFlashCardDto
    {
        public string? Word { get; set; }
        public string? Meaning { get; set; }
        public string? Pronunciation { get; set; }
        public int? ModuleId { get; set; }

        public string? PartOfSpeech { get; set; }
        public string? Example { get; set; }
        public string? ExampleTranslation { get; set; }
        public string? Synonyms { get; set; }
        public string? Antonyms { get; set; }

        public string? ImageTempKey { get; set; }
        public string? AudioTempKey { get; set; }
        public string? ImageType { get; set; }
        public string? AudioType { get; set; }
    }

    // DTO toạ nhiều flash card
    public class BulkImportFlashCardDto
    {
        public int ModuleId { get; set; }
        public List<CreateFlashCardDto> FlashCards { get; set; } = new();
        public bool ReplaceExisting { get; set; } = false;
    }

    // DTO chuyển hướng flash card với phân trang
    public class PaginatedFlashCardDto
    {
        public FlashCardDto? FlashCard { get; set; }
        public int CurrentIndex { get; set; }
        public int TotalCards { get; set; }
        public bool HasPrevious => CurrentIndex > 1;
        public bool HasNext => CurrentIndex < TotalCards;
    }
}
