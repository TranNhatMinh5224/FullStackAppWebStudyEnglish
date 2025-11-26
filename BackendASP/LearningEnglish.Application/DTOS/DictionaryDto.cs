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

    // Request DTO for auto-generate FlashCard from word
    public class GenerateFlashCardRequestDto
    {
        public string Word { get; set; } = string.Empty;
    
        public bool TranslateToVietnamese { get; set; } = true;
    }

    // Response DTO for generate FlashCard preview
    public class GenerateFlashCardPreviewResponseDto
    {
        public string Word { get; set; } = string.Empty;
        public string? Meaning { get; set; }
        public string? Pronunciation { get; set; }
        public string? PartOfSpeech { get; set; }
        public string? Example { get; set; }
        public string? ExampleTranslation { get; set; }
        public string? Synonyms { get; set; }
        public string? Antonyms { get; set; }
        
        // URLs for preview
        public string? AudioUrl { get; set; }
        public string? ImageUrl { get; set; }
        
        // Temp keys for create operation
        public string? AudioTempKey { get; set; }
        public string? ImageTempKey { get; set; }
    }
}
