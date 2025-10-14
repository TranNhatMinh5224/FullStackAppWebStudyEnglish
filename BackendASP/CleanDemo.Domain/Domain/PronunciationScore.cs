namespace CleanDemo.Domain.Entities;

public class PronunciationScore
{
    public int PronunciationScoreId { get; set; }
    public int UserId { get; set; }
    public int VocabularyId { get; set; }
    public double Score { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public User? User { get; set; }
    public Vocabulary? Vocabulary { get; set; }
}
