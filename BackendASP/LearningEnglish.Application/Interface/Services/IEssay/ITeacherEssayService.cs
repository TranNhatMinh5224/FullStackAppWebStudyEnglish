using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface.Services.Essay
{
    public interface ITeacherEssayService
    {
        // Teacher tạo essay (cần kiểm tra ownership: assessment -> lesson -> course -> teacherId)
        Task<ServiceResponse<EssayDto>> TeacherCreateEssay(CreateEssayDto dto, int teacherId);
        
        // Teacher lấy thông tin essay (có thể xem nếu owner HOẶC đã enroll)
        Task<ServiceResponse<EssayDto>> GetEssayByIdAsync(int essayId, int teacherId);
        
        // Teacher lấy danh sách essay theo assessment (có thể xem nếu owner HOẶC đã enroll)
        Task<ServiceResponse<List<EssayDto>>> GetEssaysByAssessmentIdAsync(int assessmentId, int teacherId);
        
        // Teacher cập nhật essay (cần kiểm tra ownership)
        Task<ServiceResponse<EssayDto>> UpdateEssay(int essayId, UpdateEssayDto dto, int teacherId);
        
        // Teacher xóa essay (cần kiểm tra ownership)
        Task<ServiceResponse<bool>> DeleteEssay(int essayId, int teacherId);
    }
}
