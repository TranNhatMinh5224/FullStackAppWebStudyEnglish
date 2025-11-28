using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IEssaySubmissionService
    {
        // Crud cho Essay Submission 
        Task<ServiceResponse<EssaySubmissionDto>> CreateSubmissionAsync(CreateEssaySubmissionDto dto, int userId);
        Task<ServiceResponse<EssaySubmissionDto>> GetSubmissionByIdAsync(int submissionId);
        Task<ServiceResponse<List<EssaySubmissionDto>>> GetSubmissionsByUserIdAsync(int userId);
        Task<ServiceResponse<List<EssaySubmissionDto>>> GetSubmissionsByAssessmentIdAsync(int assessmentId, int? teacherId = null);
        Task<ServiceResponse<EssaySubmissionDto?>> GetUserSubmissionForEssayAsync(int userId, int essayId);
        Task<ServiceResponse<EssaySubmissionDto>> UpdateSubmissionAsync(int submissionId, UpdateEssaySubmissionDto dto, int userId);
        Task<ServiceResponse<bool>> DeleteSubmissionAsync(int submissionId, int userId);
    }
}