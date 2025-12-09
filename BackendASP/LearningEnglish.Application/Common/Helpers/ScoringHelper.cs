using System.Text.Json;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Service.ScoringStrategies
{
    public static class ScoringHelper
    {
        // Parse CorrectAnswersJson thành List<int> (cho Ordering)
        public static List<int>? ParseCorrectOrder(string? json, List<AnswerOption> options)
        {
            if (string.IsNullOrEmpty(json) || options == null) return null;
            try
            {
                // Parse correctAnswersJson: ["Wake up", "Brush teeth", "Eat breakfast", "Go to school"]
                var correctOrderStrings = JsonSerializer.Deserialize<List<string>>(json);
                if (correctOrderStrings == null) return null;

                // Convert text list thành option ID list
                var result = new List<int>();
                foreach (var text in correctOrderStrings)
                {
                    var option = options.FirstOrDefault(o => o.Text == text);
                    if (option != null)
                    {
                        result.Add(option.AnswerOptionId);
                    }
                }

                return result;
            }
            catch (JsonException)
            {
                return null;  // Invalid JSON
            }
        }

        // Parse CorrectAnswersJson thành Dictionary<int, int> (cho Matching)
        public static Dictionary<int, int>? ParseCorrectMatches(string? json, string? metadataJson, List<AnswerOption> options)
        {
            if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(metadataJson) || options == null)
                return null;

            try
            {
                // Parse correctAnswersJson: {"hello": "a greeting", "book": "something to read"}
                var correctMatches = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (correctMatches == null) return null;

                // Parse metadataJson: {"left": ["hello", "book"], "right": ["a greeting", "something to read"]}
                var metadata = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(metadataJson);
                if (metadata == null || !metadata.TryGetValue("left", out List<string>? leftTexts) || !metadata.TryGetValue("right", out List<string>? rightTexts))
                    return null;

                // Tạo mapping từ left text sang right text index
                var textToRightIndex = new Dictionary<string, int>();
                for (int i = 0; i < rightTexts.Count; i++)
                {
                    textToRightIndex[rightTexts[i]] = i;
                }

                // Tạo mapping từ left option ID sang right option ID
                var result = new Dictionary<int, int>();
                foreach (var leftText in leftTexts)
                {
                    if (correctMatches.TryGetValue(leftText, out var rightText))
                    {
                        // Tìm option ID cho left text
                        var leftOption = options.FirstOrDefault(o => o.Text == leftText);
                        if (leftOption == null) continue;

                        // Tìm option ID cho right text
                        var rightOption = options.FirstOrDefault(o => o.Text == rightText);
                        if (rightOption == null) continue;

                        result[leftOption.AnswerOptionId] = rightOption.AnswerOptionId;
                    }
                }

                return result;
            }
            catch (JsonException)
            {
                return null;  // Invalid JSON
            }
        }

        // Có thể thêm methods khác cho FillBlank, etc.
    }
}
