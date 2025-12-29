using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface ITeacherAssessmentService
    {
        // Tạo assessment (Teacher - validate module ownership)
        Task<ServiceResponse<AssessmentDto>> CreateAssessmentAsync(CreateAssessmentDto dto, int teacherId);
        
        // Lấy danh sách assessment theo module (validate ownership)
        Task<ServiceResponse<List<AssessmentDto>>> GetAssessmentsByModuleIdAsync(int moduleId, int teacherId);
        
        // Lấy thông tin assessment (validate ownership)
        Task<ServiceResponse<AssessmentDto>> GetAssessmentByIdAsync(int assessmentId, int teacherId);
        
        // Cập nhật assessment (validate ownership)
        Task<ServiceResponse<AssessmentDto>> UpdateAssessmentAsync(int assessmentId, UpdateAssessmentDto dto, int teacherId);
        
        // Xóa assessment (validate ownership)
        Task<ServiceResponse<bool>> DeleteAssessmentAsync(int assessmentId, int teacherId);
    }
}
