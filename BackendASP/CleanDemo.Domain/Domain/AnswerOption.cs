namespace CleanDemo.Domain.Entities;

public class AnswerOption
{
    public int AnswerOptionId { get; set; }
    public string Text { get; set; } = string.Empty;
    
    public bool IsCorrect { get; set; }

    public int QuestionId { get; set; }
    
    // Navigation Properties
    public Question? Question { get; set; }
}
