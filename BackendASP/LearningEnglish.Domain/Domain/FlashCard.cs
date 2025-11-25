using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Domain.Entities;

public class FlashCard
{
    public int FlashCardId { get; set; }
    public int? ModuleId { get; set; }

    // Thông tin cốt lõi của từ vựng
    public string Word { get; set; } = string.Empty;              // "Beautiful"
    public string Meaning { get; set; } = string.Empty;           // "Đẹp, xinh đẹp"
    public string? Pronunciation { get; set; }                    // "/ˈbjuːtɪfl/"
    public string? ImageUrl { get; set; }
    public string? AudioUrl { get; set; }

    public string? ImageType { get; set; }
    public string? AudioType { get; set; }
    
    // Thông tin bổ sung
    public string? PartOfSpeech { get; set; }               // Từ loại: Noun, Verb, Adjective...
    public string? Example { get; set; }                          // "She is a beautiful woman"
    public string? ExampleTranslation { get; set; }               // "Cô ấy là một người phụ nữ xinh đẹp"
    public string? Synonyms { get; set; }                         // "pretty, gorgeous, lovely" (JSON array string)
    public string? Antonyms { get; set; }                         // "ugly, unattractive" (JSON array string)


    // Metadata cơ bản
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public Module? Module { get; set; }
    public List<FlashCardReview> Reviews { get; set; } = new();
    public List<PronunciationAssessment> PronunciationAssessments { get; set; } = new();
}

