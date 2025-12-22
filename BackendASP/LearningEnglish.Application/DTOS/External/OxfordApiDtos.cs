using System.Text.Json.Serialization;

namespace LearningEnglish.Application.DTOs
{
    public class OxfordApiResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("results")]
        public List<OxfordResult>? Results { get; set; }
    }

    public class OxfordResult
    {
        [JsonPropertyName("lexicalEntries")]
        public List<OxfordLexicalEntry>? LexicalEntries { get; set; }
    }

    public class OxfordLexicalEntry
    {
        [JsonPropertyName("lexicalCategory")]
        public OxfordLexicalCategory? LexicalCategory { get; set; }

        [JsonPropertyName("pronunciations")]
        public List<OxfordPronunciation>? Pronunciations { get; set; }

        [JsonPropertyName("entries")]
        public List<OxfordEntry>? Entries { get; set; }
    }

    public class OxfordLexicalCategory
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    public class OxfordPronunciation
    {
        [JsonPropertyName("phoneticSpelling")]
        public string? PhoneticSpelling { get; set; }

        [JsonPropertyName("audioFile")]
        public string? AudioFile { get; set; }
    }

    public class OxfordEntry
    {
        [JsonPropertyName("senses")]
        public List<OxfordSense>? Senses { get; set; }
    }

    public class OxfordSense
    {
        [JsonPropertyName("definitions")]
        public List<string>? Definitions { get; set; }

        [JsonPropertyName("examples")]
        public List<OxfordExample>? Examples { get; set; }

        [JsonPropertyName("synonyms")]
        public List<OxfordSynonym>? Synonyms { get; set; }

        [JsonPropertyName("antonyms")]
        public List<OxfordAntonym>? Antonyms { get; set; }
    }

    public class OxfordSynonym
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    public class OxfordAntonym
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    public class OxfordExample
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}