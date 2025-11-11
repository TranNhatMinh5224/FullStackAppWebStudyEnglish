using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IAssessmentService
    {
        Task<ServiceResponse<AssessmentDto>> CreateAssessment(CreateAssessmentDto dto, int? teacherId = null);
        Task<ServiceResponse<List<AssessmentDto>>> GetAssessmentsByModuleId(int moduleId);
        Task<ServiceResponse<AssessmentDto>> GetAssessmentById(int assessmentId);
        Task<ServiceResponse<AssessmentDto>> UpdateAssessment(int assessmentId, UpdateAssessmentDto dto);
        Task<ServiceResponse<bool>> DeleteAssessment(int assessmentId, int? teacherId = null);
    }

}
