namespace CleanDemo.Domain.Entities;

public class Vocabulary
{
    public int VocabularyId { get; set; }
    public string Word { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? Pronunciation { get; set; }
    public string? ImageUrl { get; set; }
    public string? AudioUrl { get; set; }

    public int LessonId { get; set; }
    
    // Navigation Properties
    public Lesson? Lesson { get; set; }
    public List<ReviewWord> ReviewWords { get; set; } = new();
    public List<PronunciationScore> PronunciationScores { get; set; } = new();
}
