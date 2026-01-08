using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LearningEnglish.Application.Service.AssessmentService;

public class AiResponseParser : IAiResponseParser
{
    private readonly ILogger<AiResponseParser> _logger;

    public AiResponseParser(ILogger<AiResponseParser> logger)
    {
        _logger = logger;
    }

    public AiGradingResult ParseGradingResponse(string content)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("AI response content is null or empty", nameof(content));
            }

            // Extract JSON from response (handle markdown code blocks)
            var jsonContent = ExtractJsonFromResponse(content);

            // Deserialize JSON
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };

            var result = JsonSerializer.Deserialize<AiGradingResult>(jsonContent, options);

            if (result == null)
            {
                throw new InvalidOperationException("Failed to deserialize AI response to AiGradingResult");
            }

            // Validate required fields
            ValidateGradingResult(result);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse AI grading response. Content: {Content}", content);
            
            // Return safe default instead of throwing
            return new AiGradingResult
            {
                Score = 0,
                Feedback = "Error parsing AI response. Please grade manually.",
                Breakdown = new GradingBreakdown
                {
                    ContentScore = 0,
                    LanguageScore = 0,
                    OrganizationScore = 0,
                    MechanicsScore = 0
                },
                Strengths = new List<string> { "Unable to analyze" },
                Improvements = new List<string> { "Unable to analyze" }
            };
        }
    }

    private string ExtractJsonFromResponse(string content)
    {
        var jsonText = content.Trim();

        // Remove markdown code blocks if present
        if (jsonText.Contains("```json"))
        {
            var startIndex = jsonText.IndexOf("```json") + 7;
            var endIndex = jsonText.LastIndexOf("```");
            if (endIndex > startIndex)
            {
                jsonText = jsonText.Substring(startIndex, endIndex - startIndex).Trim();
            }
        }
        else if (jsonText.Contains("```"))
        {
            var startIndex = jsonText.IndexOf("```") + 3;
            var endIndex = jsonText.LastIndexOf("```");
            if (endIndex > startIndex)
            {
                jsonText = jsonText.Substring(startIndex, endIndex - startIndex).Trim();
            }
        }

        // Extract JSON object if there's extra text
        var jsonStart = jsonText.IndexOf('{');
        var jsonEnd = jsonText.LastIndexOf('}');

        if (jsonStart == -1 || jsonEnd == -1 || jsonEnd <= jsonStart)
        {
            throw new FormatException("No valid JSON object found in AI response");
        }

        return jsonText.Substring(jsonStart, jsonEnd - jsonStart + 1);
    }

    /// <summary>
    /// Validates that the grading result has required fields
    /// </summary>
    private void ValidateGradingResult(AiGradingResult result)
    {
        if (result.Score < 0)
        {
            _logger.LogWarning("AI returned negative score: {Score}, setting to 0", result.Score);
            result.Score = 0;
        }

        if (string.IsNullOrWhiteSpace(result.Feedback))
        {
            _logger.LogWarning("AI returned empty feedback, using default");
            result.Feedback = "No feedback provided.";
        }

        if (result.Strengths == null || result.Strengths.Count == 0)
        {
            _logger.LogWarning("AI returned no strengths, using default");
            result.Strengths = new List<string> { "Unable to identify specific strengths" };
        }

        if (result.Improvements == null || result.Improvements.Count == 0)
        {
            _logger.LogWarning("AI returned no improvements, using default");
            result.Improvements = new List<string> { "Unable to identify specific improvements" };
        }

        // Ensure breakdown exists
        if (result.Breakdown == null)
        {
            _logger.LogWarning("AI returned no breakdown, creating default");
            result.Breakdown = new GradingBreakdown
            {
                ContentScore = 0,
                LanguageScore = 0,
                OrganizationScore = 0,
                MechanicsScore = 0
            };
        }
    }
}

