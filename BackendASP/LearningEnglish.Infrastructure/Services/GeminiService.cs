using LearningEnglish.Application.Cofigurations;
using LearningEnglish.Application.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace LearningEnglish.Infrastructure.Services.ExternalProviders;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(
        HttpClient httpClient,
        IOptions<GeminiOptions> options,
        ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<GeminiResponse> GenerateContentAsync(string prompt, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_options.BaseUrl}/models/{_options.Model}:generateContent?key={_options.ApiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = _options.Temperature,
                    maxOutputTokens = _options.MaxTokens
                }
            };

            _logger.LogInformation("🤖 Calling Gemini API...");

            var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("❌ Gemini API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return new GeminiResponse
                {
                    Success = false,
                    ErrorMessage = $"API returned {response.StatusCode}: {errorContent}"
                };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            var geminiResult = JsonSerializer.Deserialize<GeminiApiResponse>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var content = geminiResult?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? string.Empty;

            _logger.LogInformation("✅ Gemini API response received successfully");

            return new GeminiResponse
            {
                Success = true,
                Content = content
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Exception calling Gemini API");
            return new GeminiResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}

// Internal DTOs for Gemini API response parsing
internal class GeminiApiResponse
{
    public List<Candidate>? Candidates { get; set; }
}

internal class Candidate
{
    public Content? Content { get; set; }
}

internal class Content
{
    public List<Part>? Parts { get; set; }
}

internal class Part
{
    public string? Text { get; set; }
}
