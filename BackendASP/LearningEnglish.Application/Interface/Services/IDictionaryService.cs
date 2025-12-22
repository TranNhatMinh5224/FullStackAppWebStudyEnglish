using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IDictionaryService
    {
        // Tra từ điển
        Task<ServiceResponse<DictionaryLookupResultDto>> LookupWordAsync(string word, string? targetLanguage = "vi");

        // Tạo flashcard từ từ vựng
        Task<ServiceResponse<GenerateFlashCardPreviewResponseDto>> GenerateFlashCardFromWordAsync(string word);

    }
}
