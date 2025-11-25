namespace LearningEnglish.Application.DTOs
{
    // Dictionary API lookup result
    public class DictionaryLookupResultDto
    {
        public string Word { get; set; } = string.Empty;
        public string? Phonetic { get; set; }
        public List<DictionaryMeaningDto> Meanings { get; set; } = new();
        public string? SourceUrl { get; set; }
        public string? AudioUrl { get; set; } // Audio pronunciation URL from dictionary API
    }

    public class DictionaryMeaningDto
    {
        public string PartOfSpeech { get; set; } = string.Empty;
        public List<DictionaryDefinitionDto> Definitions { get; set; } = new();
        public List<string> Synonyms { get; set; } = new();
        public List<string> Antonyms { get; set; } = new();
    }

    public class DictionaryDefinitionDto
    {
        public string Definition { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    // Request DTO for word lookup
    public class WordLookupRequestDto
    {
        public string Word { get; set; } = string.Empty;
        public string? TargetLanguage { get; set; } = "vi"; // Vietnamese translation
    }

    // Request DTO for batch lookup
    public class BatchWordLookupRequestDto
    {
        public List<string> Words { get; set; } = new();
        public string? TargetLanguage { get; set; } = "vi";
    }

    // Request DTO for auto-generate FlashCard from word
    public class GenerateFlashCardRequestDto
    {
        public string Word { get; set; } = string.Empty;
        public int? ModuleId { get; set; }
        public bool TranslateToVietnamese { get; set; } = true;
    }
}
