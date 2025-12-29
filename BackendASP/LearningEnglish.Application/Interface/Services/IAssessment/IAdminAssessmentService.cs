using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IAdminAssessmentService
    {
        // Tạo assessment (Admin - không cần teacherId)
        Task<ServiceResponse<AssessmentDto>> CreateAssessmentAsync(CreateAssessmentDto dto);
        
        // Lấy danh sách assessment theo module
        Task<ServiceResponse<List<AssessmentDto>>> GetAssessmentsByModuleIdAsync(int moduleId);
        
        // Lấy thông tin assessment
        Task<ServiceResponse<AssessmentDto>> GetAssessmentByIdAsync(int assessmentId);
        
        // Cập nhật assessment
        Task<ServiceResponse<AssessmentDto>> UpdateAssessmentAsync(int assessmentId, UpdateAssessmentDto dto);
        
        // Xóa assessment (không check ownership)
        Task<ServiceResponse<bool>> DeleteAssessmentAsync(int assessmentId);
    }
}
