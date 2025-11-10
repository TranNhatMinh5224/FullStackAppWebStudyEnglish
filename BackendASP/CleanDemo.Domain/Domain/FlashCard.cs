namespace CleanDemo.Domain.Entities;

// FlashCard đơn giản - chỉ lưu thông tin cơ bản của từ vựng
public class FlashCard
{
    public int VocabularyId { get; set; }
    public int? ModuleId { get; set; }

    // Thông tin cốt lõi của từ vựng
    public string Word { get; set; } = string.Empty;              // "Beautiful"
    public string Meaning { get; set; } = string.Empty;           // "Đẹp, xinh đẹp"
    public string? Pronunciation { get; set; }                    // "/ˈbjuːtɪfl/"
    public string? ImageUrl { get; set; }                         // Hình ảnh minh họa
    public string? AudioUrl { get; set; }                         // File phát âm

    // Metadata cơ bản
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public Module? Module { get; set; }
    public List<FlashCardReview> Reviews { get; set; } = new();
    public List<PronunciationAssessment> PronunciationAssessments { get; set; } = new();
}

