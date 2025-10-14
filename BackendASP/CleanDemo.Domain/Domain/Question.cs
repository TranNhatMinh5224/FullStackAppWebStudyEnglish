namespace CleanDemo.Domain.Entities;

public class Question
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;

    public int MiniTestId { get; set; }
    
    // Navigation Properties
    public MiniTest? MiniTest { get; set; }
    public List<AnswerOption> Options { get; set; } = new();
}
