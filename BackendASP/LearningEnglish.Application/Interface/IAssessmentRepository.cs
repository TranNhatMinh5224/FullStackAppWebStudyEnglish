using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IAssessmentRepository
    {
        Task AddAssessment(Assessment assessment);
        Task<Assessment?> GetAssessmentById(int assessmentId);

        Task<List<Assessment>> GetAssessmentsByModuleId(int moduleId);
        Task UpdateAssessment(Assessment assessment);

        Task DeleteAssessment(int assessmentId);
        Task<bool> ModuleExists(int moduleId);

        Task<bool> IsTeacherOwnerOfModule(int teacherId, int moduleId);
    }
}
