using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IDictionaryService
    {

        Task<ServiceResponse<DictionaryLookupResultDto>> LookupWordAsync(string word, string? targetLanguage = "vi");
        

        Task<ServiceResponse<GenerateFlashCardPreviewResponseDto>> GenerateFlashCardFromWordAsync(string word);

    }
}
