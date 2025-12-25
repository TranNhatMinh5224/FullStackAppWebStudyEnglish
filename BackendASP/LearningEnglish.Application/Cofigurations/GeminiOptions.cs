namespace LearningEnglish.Application.Cofigurations;

public class GeminiOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-1.5-flash";
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
    public int MaxTokens { get; set; } = 2048;
    public double Temperature { get; set; } = 0.3;
}
