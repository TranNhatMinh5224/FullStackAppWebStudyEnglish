using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IAssessmentService
    {
        // Tạo assessment
        Task<ServiceResponse<AssessmentDto>> CreateAssessment(CreateAssessmentDto dto, int? teacherId = null);
        
        // Lấy danh sách assessment theo module
        Task<ServiceResponse<List<AssessmentDto>>> GetAssessmentsByModuleId(int moduleId);
        
        // Lấy thông tin assessment
        Task<ServiceResponse<AssessmentDto>> GetAssessmentById(int assessmentId);
        
        // Cập nhật assessment
        Task<ServiceResponse<AssessmentDto>> UpdateAssessment(int assessmentId, UpdateAssessmentDto dto);
        
        // Xóa assessment
        Task<ServiceResponse<bool>> DeleteAssessment(int assessmentId, int? teacherId = null);
    }

}
