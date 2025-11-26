using System.Text.Json.Serialization;

namespace LearningEnglish.Application.DTOs
{
    public class UnsplashSearchResponse
    {
        [JsonPropertyName("results")]
        public List<UnsplashResult>? Results { get; set; }
    }

    public class UnsplashResult
    {
        [JsonPropertyName("urls")]
        public UnsplashUrls? Urls { get; set; }
    }

    public class UnsplashUrls
    {
        [JsonPropertyName("regular")]
        public string? Regular { get; set; }

        [JsonPropertyName("small")]
        public string? Small { get; set; }

        [JsonPropertyName("thumb")]
        public string? Thumb { get; set; }
    }
}