using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{

    public interface IPronunciationAssessmentService
    {
        // Tạo đánh giá phát âm
        Task<ServiceResponse<PronunciationAssessmentDto>> CreateAssessmentAsync(
            CreatePronunciationAssessmentDto dto,
            int userId);

        // Lấy flashcard với tiến độ phát âm
        Task<ServiceResponse<List<FlashCardWithPronunciationDto>>> GetFlashCardsWithPronunciationProgressAsync(
            int moduleId,
            int userId);

        // Lấy tổng hợp phát âm của module
        Task<ServiceResponse<ModulePronunciationSummaryDto>> GetModulePronunciationSummaryAsync(
            int moduleId,
            int userId);
    }
}