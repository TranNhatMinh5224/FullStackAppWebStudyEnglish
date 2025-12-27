using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IEssayService
    {
        // Tạo essay
        Task<ServiceResponse<EssayDto>> CreateEssayAsync(CreateEssayDto dto, int? teacherId = null);
        
        // Lấy thông tin essay
        Task<ServiceResponse<EssayDto>> GetEssayByIdAsync(int essayId);
        
        // Lấy danh sách essay theo assessment
        Task<ServiceResponse<List<EssayDto>>> GetEssaysByAssessmentIdAsync(int assessmentId);
        
        // Cập nhật essay
        Task<ServiceResponse<EssayDto>> UpdateEssayAsync(int essayId, UpdateEssayDto dto, int? teacherId = null);
        
        // Xóa essay
        Task<ServiceResponse<bool>> DeleteEssayAsync(int essayId, int? teacherId = null);
    }
}