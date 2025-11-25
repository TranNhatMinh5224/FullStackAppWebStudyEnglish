using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IPronunciationAssessmentService
    {
        /// <summary>
        /// Create new pronunciation assessment with Azure AI
        /// </summary>
        Task<ServiceResponse<PronunciationAssessmentDto>> CreateAssessmentAsync(
            CreatePronunciationAssessmentDto dto, 
            int userId);

        /// <summary>
        /// Get assessment by ID
        /// </summary>
        Task<ServiceResponse<PronunciationAssessmentDto>> GetAssessmentByIdAsync(int id, int userId);

        /// <summary>
        /// Get all assessments by user
        /// </summary>
        Task<ServiceResponse<List<ListPronunciationAssessmentDto>>> GetUserAssessmentsAsync(int userId);

        /// <summary>
        /// Get assessments by FlashCard
        /// </summary>
        Task<ServiceResponse<List<ListPronunciationAssessmentDto>>> GetFlashCardAssessmentsAsync(
            int flashCardId, 
            int userId);

        /// <summary>
        /// Delete assessment
        /// </summary>
        Task<ServiceResponse<bool>> DeleteAssessmentAsync(int id, int userId);

        /// <summary>
        /// Get user statistics
        /// </summary>
        Task<ServiceResponse<object>> GetUserStatisticsAsync(int userId);
    }
}
