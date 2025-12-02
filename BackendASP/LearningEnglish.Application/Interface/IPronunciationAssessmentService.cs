using LearningEnglish.Application.Common;
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
    }
}