namespace LearningEnglish.Application.Cofigurations;

/// <summary>
/// Configuration options for Gemini AI service
/// </summary>
public class GeminiOptions
{
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Gemini model to use. Options: gemini-2.0-flash-exp, gemini-1.5-pro, gemini-1.5-flash
    /// </summary>
    public string Model { get; set; } = "gemini-2.0-flash-exp";
    
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
    
    /// <summary>
    /// Maximum tokens in response (default: 2048)
    /// </summary>
    public int MaxTokens { get; set; } = 2048;
    
    /// <summary>
    /// Temperature for response randomness (0.0-1.0). Lower = more deterministic (default: 0.3)
    /// </summary>
    public double Temperature { get; set; } = 0.3;
}
