using System.Text.Json;

namespace LearningEnglish.Application.Service.ScoringStrategies
{
    public static class ScoringHelper
    {
        // Parse CorrectAnswersJson thành List<int> (cho Ordering)
        public static List<int>? ParseCorrectOrder(string? json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                var correctOrderStrings = JsonSerializer.Deserialize<List<string>>(json);
                return correctOrderStrings?.Select(int.Parse).ToList();
            }
            catch (JsonException)
            {
                return null;  // Invalid JSON
            }
        }

        // Parse CorrectAnswersJson thành Dictionary<int, int> (cho Matching)
        public static Dictionary<int, int>? ParseCorrectMatches(string? json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                var correctMatchesStrings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                return correctMatchesStrings?.ToDictionary(k => int.Parse(k.Key), v => int.Parse(v.Value));
            }
            catch (JsonException)
            {
                return null;  // Invalid JSON
            }
        }

        // Có thể thêm methods khác cho FillBlank, etc.
    }
}
