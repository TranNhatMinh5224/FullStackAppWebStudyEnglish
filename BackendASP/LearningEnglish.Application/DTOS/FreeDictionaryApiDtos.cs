using System.Text.Json.Serialization;

namespace LearningEnglish.Application.DTOs
{
    public class DictionaryApiResponse
    {
        [JsonPropertyName("word")]
        public string? Word { get; set; }

        [JsonPropertyName("phonetic")]
        public string? Phonetic { get; set; }

        [JsonPropertyName("phonetics")]
        public List<DictionaryPhonetic>? Phonetics { get; set; }

        [JsonPropertyName("meanings")]
        public List<DictionaryMeaning>? Meanings { get; set; }

        [JsonPropertyName("sourceUrls")]
        public List<string>? SourceUrls { get; set; }
    }

    public class DictionaryPhonetic
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("audio")]
        public string? Audio { get; set; }
    }

    public class DictionaryMeaning
    {
        [JsonPropertyName("partOfSpeech")]
        public string? PartOfSpeech { get; set; }

        [JsonPropertyName("definitions")]
        public List<DictionaryDefinition>? Definitions { get; set; }

        [JsonPropertyName("synonyms")]
        public List<string>? Synonyms { get; set; }

        [JsonPropertyName("antonyms")]
        public List<string>? Antonyms { get; set; }
    }

    public class DictionaryDefinition
    {
        [JsonPropertyName("definition")]
        public string? Definition { get; set; }

        [JsonPropertyName("example")]
        public string? Example { get; set; }
    }
}