using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface.Services.Essay
{
    public interface IAdminEssayService
    {
        // Admin tạo essay
        Task<ServiceResponse<EssayDto>> AdminCreateEssay(CreateEssayDto dto);
        
        // Admin lấy thông tin essay
        Task<ServiceResponse<EssayDto>> GetEssayByIdAsync(int essayId);
        
        // Admin lấy danh sách essay theo assessment
        Task<ServiceResponse<List<EssayDto>>> GetEssaysByAssessmentIdAsync(int assessmentId);
        
        // Admin cập nhật essay
        Task<ServiceResponse<EssayDto>> UpdateEssay(int essayId, UpdateEssayDto dto);
        
        // Admin xóa essay
        Task<ServiceResponse<bool>> DeleteEssay(int essayId);
    }
}
