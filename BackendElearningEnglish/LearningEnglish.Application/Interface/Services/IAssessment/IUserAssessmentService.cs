using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IUserAssessmentService
    {
        // Lấy danh sách assessment theo module (User chỉ xem)
        Task<ServiceResponse<List<AssessmentDto>>> GetAssessmentsByModuleIdAsync(int moduleId, int userId);
        
        // Lấy thông tin assessment (User chỉ xem)
        Task<ServiceResponse<AssessmentDto>> GetAssessmentByIdAsync(int assessmentId, int userId);
    }
}
