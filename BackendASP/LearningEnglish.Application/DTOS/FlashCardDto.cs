namespace LearningEnglish.Application.DTOs
{
    // DTO for FlashCard response
    public class FlashCardDto
    {
        public int FlashCardId { get; set; }
        public int? ModuleId { get; set; }
        public string Word { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public string? Pronunciation { get; set; }
        public string? ImageUrl { get; set; }
        public string? AudioUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation info
        public string? ModuleName { get; set; }

        // Review statistics
        public int ReviewCount { get; set; }
        public decimal SuccessRate { get; set; }
        public DateTime? LastReviewedAt { get; set; }
        public DateTime? NextReviewAt { get; set; }
        public int CurrentLevel { get; set; } // SRS level (0-6)
    }

    // DTO for listing flashcards (lighter version)
    public class ListFlashCardDto
    {
        public int FlashCardId { get; set; }
        public int? ModuleId { get; set; }
        public string Word { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public string? Pronunciation { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ReviewCount { get; set; }
        public decimal SuccessRate { get; set; }
        public int CurrentLevel { get; set; }
    }

    // DTO for creating new flashcard
    public class CreateFlashCardDto
    {
        public int? ModuleId { get; set; }
        public string Word { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public string? Pronunciation { get; set; }
        public string? ImageUrl { get; set; }
        public string? AudioUrl { get; set; }
    }

    // DTO for updating existing flashcard
    public class UpdateFlashCardDto
    {
        public string? Word { get; set; }
        public string? Meaning { get; set; }
        public string? Pronunciation { get; set; }
        public string? ImageUrl { get; set; }
        public string? AudioUrl { get; set; }
        public int? ModuleId { get; set; }
    }

    // DTO for flashcard with user progress info
    public class FlashCardWithProgressDto : FlashCardDto
    {
        public bool IsLearned { get; set; }
        public bool NeedsReview { get; set; }
        public int ConsecutiveCorrect { get; set; }
        public float EasinessFactor { get; set; }
        public int IntervalDays { get; set; }
    }

    // DTO for bulk flashcard import
    public class BulkImportFlashCardDto
    {
        public int ModuleId { get; set; }
        public List<CreateFlashCardDto> FlashCards { get; set; } = new();
        public bool ReplaceExisting { get; set; } = false;
    }
}
