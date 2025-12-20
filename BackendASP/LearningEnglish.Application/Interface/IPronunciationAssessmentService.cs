using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    /// <summary>
    /// Realtime-only pronunciation assessment service
    /// NO individual assessments stored - only aggregated progress
    /// </summary>
    public interface IPronunciationAssessmentService
    {
        /// <summary>
        /// Create realtime pronunciation assessment (not stored, only progress updated)
        /// </summary>
        Task<ServiceResponse<PronunciationAssessmentDto>> CreateAssessmentAsync(
            CreatePronunciationAssessmentDto dto,
            int userId);

        /// <summary>
        /// Get flashcards with pronunciation progress for a module
        /// </summary>
        Task<ServiceResponse<List<FlashCardWithPronunciationDto>>> GetFlashCardsWithPronunciationProgressAsync(
            int moduleId,
            int userId);

        /// <summary>
        /// Get paginated list of flashcards with pronunciation progress for list view
        /// </summary>
        Task<ServiceResponse<PagedResult<FlashCardWithPronunciationDto>>> GetFlashCardsWithPronunciationProgressPaginatedAsync(
            int moduleId,
            int userId,
            PageRequest request);

        /// <summary>
        /// Get pronunciation summary/statistics for a module
        /// </summary>
        Task<ServiceResponse<ModulePronunciationSummaryDto>> GetModulePronunciationSummaryAsync(
            int moduleId,
            int userId);
    }
}