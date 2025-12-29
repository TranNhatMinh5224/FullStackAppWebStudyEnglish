using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface.Services.Essay
{
    public interface IUserEssayService
    {
        // Lấy thông tin essay (với userId để track progress trong tương lai)
        Task<ServiceResponse<EssayDto>> GetEssayByIdAsync(int essayId, int userId);
        
        // Lấy danh sách essay theo assessment (với userId để track progress)
        Task<ServiceResponse<List<EssayDto>>> GetEssaysByAssessmentIdAsync(int assessmentId, int userId);
    }
}
