using System.Text.Json;  // sử dung de xu ly JSON
namespace LearningEnglish.Application.Common.Helpers
{
    public static class QuestionAnswerParser
    {
        public static List<string> ParseCorrectAnswers(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return new List<string>();

            raw = raw.Trim();

            try
            {
                if (raw.StartsWith("["))
                {
                    return JsonSerializer.Deserialize<List<string>>(raw)
                           ?? new List<string>();
                }

                if (raw.StartsWith("\""))
                {
                    var one = JsonSerializer.Deserialize<string>(raw);
                    return one != null ? new List<string> { one } : new List<string>();
                }

                return new List<string> { raw };
            }
            catch
            {
                return new List<string> { raw };
            }
        }
    }
}