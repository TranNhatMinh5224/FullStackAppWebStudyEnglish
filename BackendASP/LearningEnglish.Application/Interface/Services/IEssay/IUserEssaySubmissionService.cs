using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IUserEssaySubmissionService
    {
        // Nộp bài essay
        Task<ServiceResponse<EssaySubmissionDto>> CreateSubmissionAsync(CreateEssaySubmissionDto dto, int userId);
        
        // Lấy bài nộp của chính mình theo ID
        Task<ServiceResponse<EssaySubmissionDto>> GetMySubmissionByIdAsync(int submissionId, int userId);
        
        // Kiểm tra đã nộp bài essay chưa
        Task<ServiceResponse<EssaySubmissionDto?>> GetMySubmissionForEssayAsync(int userId, int essayId);
        
        // Cập nhật bài nộp của chính mình
        Task<ServiceResponse<EssaySubmissionDto>> UpdateSubmissionAsync(int submissionId, UpdateEssaySubmissionDto dto, int userId);
        
        // Xóa bài nộp của chính mình
        Task<ServiceResponse<bool>> DeleteSubmissionAsync(int submissionId, int userId);
    }
}
