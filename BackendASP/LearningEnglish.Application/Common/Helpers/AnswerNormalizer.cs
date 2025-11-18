using System.Text.Json;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Common.Helpers
{
    /// <summary>
    /// Helper class để chuẩn hóa câu trả lời của user theo từng loại câu hỏi
    /// </summary>
    public static class AnswerNormalizer
    {
        /// <summary>
        /// Chuẩn hóa userAnswer theo QuestionType
        /// </summary>
        public static object? NormalizeUserAnswer(object? userAnswer, QuestionType questionType)
        {
            if (userAnswer == null)
                return null;

            // Nếu là JsonElement, convert về object trước
            if (userAnswer is JsonElement jsonElement)
            {
                userAnswer = ConvertJsonElementToObject(jsonElement);
            }

            // Xử lý theo từng loại câu hỏi
            switch (questionType)
            {
                case QuestionType.MultipleChoice:
                case QuestionType.TrueFalse:
                    return NormalizeToInt(userAnswer);

                case QuestionType.MultipleAnswers:
                    return NormalizeToListInt(userAnswer);

                case QuestionType.FillBlank:
                    return NormalizeToString(userAnswer);

                case QuestionType.Matching:
                    return NormalizeToDictionaryIntInt(userAnswer);

                case QuestionType.Ordering:
                    return NormalizeToListInt(userAnswer);

                default:
                    return userAnswer;
            }
        }

        /// <summary>
        /// Chuẩn hóa về int (cho MultipleChoice, TrueFalse)
        /// </summary>
        public static int? NormalizeToInt(object? userAnswer)
        {
            if (userAnswer == null) return null;

            if (userAnswer is int intValue)
                return intValue;
            if (userAnswer is string stringValue && int.TryParse(stringValue, out int parsedInt))
                return parsedInt;
            if (userAnswer is long longValue)
                return (int)longValue;
            if (userAnswer is decimal decimalValue)
                return (int)decimalValue;
            if (userAnswer is JsonElement je && je.TryGetInt32(out int jeInt))
                return jeInt;

            return null;
        }

        /// <summary>
        /// Chuẩn hóa về List<int> (cho MultipleAnswers, Ordering)
        /// </summary>
        public static List<int>? NormalizeToListInt(object? userAnswer)
        {
            if (userAnswer == null) return null;

            if (userAnswer is List<int> intList)
                return intList;
            
            if (userAnswer is List<object> objectList)
            {
                var convertedList = new List<int>();
                foreach (var item in objectList)
                {
                    if (item is int i)
                        convertedList.Add(i);
                    else if (item is string s && int.TryParse(s, out int parsed))
                        convertedList.Add(parsed);
                    else if (item is JsonElement je && je.TryGetInt32(out int jeInt))
                        convertedList.Add(jeInt);
                }
                return convertedList;
            }

            // Nếu là array từ JSON
            if (userAnswer is JsonElement jeArray && jeArray.ValueKind == JsonValueKind.Array)
            {
                var list = new List<int>();
                foreach (var item in jeArray.EnumerateArray())
                {
                    if (item.TryGetInt32(out int itemInt))
                        list.Add(itemInt);
                }
                return list;
            }

            return null;
        }

        /// <summary>
        /// Chuẩn hóa về string (cho FillBlank)
        /// </summary>
        public static string NormalizeToString(object? userAnswer)
        {
            if (userAnswer == null) return string.Empty;
            if (userAnswer is string str)
                return str;
            return userAnswer.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Chuẩn hóa về Dictionary<int, int> (cho Matching)
        /// </summary>
        public static Dictionary<int, int>? NormalizeToDictionaryIntInt(object? userAnswer)
        {
            if (userAnswer == null) return null;

            if (userAnswer is Dictionary<int, int> dict)
                return dict;

            if (userAnswer is Dictionary<string, object> stringDict)
            {
                var result = new Dictionary<int, int>();
                foreach (var kvp in stringDict)
                {
                    if (int.TryParse(kvp.Key, out int key) && kvp.Value != null)
                    {
                        int value = 0;
                        if (kvp.Value is int v) value = v;
                        else if (kvp.Value is string s && int.TryParse(s, out int parsed)) value = parsed;
                        else if (kvp.Value is JsonElement je && je.TryGetInt32(out int jeInt)) value = jeInt;
                        else continue;
                        
                        result[key] = value;
                    }
                }
                return result;
            }

            if (userAnswer is JsonElement jeObject && jeObject.ValueKind == JsonValueKind.Object)
            {
                var result = new Dictionary<int, int>();
                foreach (var prop in jeObject.EnumerateObject())
                {
                    if (int.TryParse(prop.Name, out int key) && prop.Value.TryGetInt32(out int value))
                    {
                        result[key] = value;
                    }
                }
                return result;
            }

            return null;
        }

        /// <summary>
        /// Convert JsonElement về object thực sự (public để dùng ở nơi khác)
        /// </summary>
        public static object ConvertJsonElementToObject(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString() ?? string.Empty;
                
                case JsonValueKind.Number:
                    // Thử parse thành int trước, nếu không được thì decimal
                    if (element.TryGetInt32(out int intValue))
                        return intValue;
                    if (element.TryGetDecimal(out decimal decimalValue))
                        return decimalValue;
                    return element.GetDouble();
                
                case JsonValueKind.True:
                    return true;
                
                case JsonValueKind.False:
                    return false;
                
                case JsonValueKind.Array:
                    // Kiểm tra xem có phải là array of numbers không (List<int>)
                    var arrayList = element.EnumerateArray().ToList();
                    if (arrayList.Count == 0)
                    {
                        return new List<int>();
                    }
                    
                    var firstElement = arrayList[0];
                    if (firstElement.ValueKind == JsonValueKind.Number)
                    {
                        var intList = new List<int>();
                        foreach (var item in arrayList)
                        {
                            if (item.TryGetInt32(out int itemValue))
                                intList.Add(itemValue);
                        }
                        return intList;
                    }
                    // Nếu là array of strings
                    if (firstElement.ValueKind == JsonValueKind.String)
                    {
                        var stringList = new List<string>();
                        foreach (var item in arrayList)
                        {
                            stringList.Add(item.GetString() ?? string.Empty);
                        }
                        return stringList;
                    }
                    // Fallback: return as JsonElement
                    return element;
                
                case JsonValueKind.Object:
                    // Nested object, return as JsonElement hoặc có thể deserialize thêm
                    return element;
                
                case JsonValueKind.Null:
                default:
                    return null!;
            }
        }

        /// <summary>
        /// Deserialize Dictionary<int, decimal> từ JSON string (cho ScoresJson)
        /// </summary>
        public static Dictionary<int, decimal> DeserializeScoresJson(string? json)
        {
            if (string.IsNullOrEmpty(json))
                return new Dictionary<int, decimal>();

            try
            {
                // Deserialize về Dictionary<string, decimal> trước (vì JSON keys luôn là string)
                var stringDict = JsonSerializer.Deserialize<Dictionary<string, decimal>>(json);
                if (stringDict == null)
                    return new Dictionary<int, decimal>();

                // Convert sang Dictionary<int, decimal>
                return stringDict.ToDictionary(
                    kvp => int.Parse(kvp.Key),
                    kvp => kvp.Value
                );
            }
            catch
            {
                return new Dictionary<int, decimal>();
            }
        }

        /// <summary>
        /// Deserialize Dictionary<int, object> từ JSON string (cho AnswersJson)
        /// </summary>
        public static Dictionary<int, object> DeserializeAnswersJson(string? json)
        {
            if (string.IsNullOrEmpty(json))
                return new Dictionary<int, object>();

            try
            {
                // Deserialize về Dictionary<string, JsonElement> trước
                var stringDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                if (stringDict == null)
                    return new Dictionary<int, object>();

                // Convert sang Dictionary<int, object>
                var result = new Dictionary<int, object>();
                foreach (var kvp in stringDict)
                {
                    var key = int.Parse(kvp.Key);
                    // Convert JsonElement về object thực sự
                    result[key] = ConvertJsonElementToObject(kvp.Value);
                }
                return result;
            }
            catch
            {
                return new Dictionary<int, object>();
            }
        }
    }
}

