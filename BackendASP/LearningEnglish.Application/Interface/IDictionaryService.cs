using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IDictionaryService
    {
        /// <summary>
        /// Lookup word from Oxford/Free Dictionary API and generate FlashCard data
        /// </summary>
        Task<ServiceResponse<DictionaryLookupResultDto>> LookupWordAsync(string word, string? targetLanguage = "vi");
        
        /// <summary>
        /// Auto-fill FlashCard data from dictionary lookup
        /// </summary>
        Task<ServiceResponse<CreateFlashCardDto>> GenerateFlashCardFromWordAsync(string word, int? moduleId = null);
        
        /// <summary>
        /// Batch lookup multiple words
        /// </summary>
        Task<ServiceResponse<List<DictionaryLookupResultDto>>> BatchLookupWordsAsync(List<string> words, string? targetLanguage = "vi");
    }
}
