namespace LearningEnglish.Application.DTOs
{
    // Dictionary API lookup result
    public class DictionaryLookupResultDto
    {
        public string Word { get; set; } = string.Empty;
        public string? Phonetic { get; set; }
        public List<DictionaryMeaningDto> Meanings { get; set; } = new();
        public string? SourceUrl { get; set; }
        public string? AudioUrl { get; set; } // url audio phát âm 
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

    // yêu cầu dto để tạo flashcard từ từ được cung cấp
    public class GenerateFlashCardRequestDto
    {
        public string Word { get; set; } = string.Empty;

        public bool TranslateToVietnamese { get; set; } = true;
    }

    // phản hồi dto để xem trước flashcard được tạo
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

        // urls cho audio và hình ảnh
        public string? AudioUrl { get; set; }
        public string? ImageUrl { get; set; }

        // keys tạm thời để truy xuất audio và hình ảnh từ storage service
        public string? AudioTempKey { get; set; }
        public string? ImageTempKey { get; set; }
    }
}
