namespace CleanDemo.Domain.Entities;

public class ReviewWord
{
    public int ReviewWordId { get; set; }
    public int UserId { get; set; }
    public int VocabularyId { get; set; }
    public DateTime ReviewAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public User? User { get; set; }
    public Vocabulary? Vocabulary { get; set; }
}
