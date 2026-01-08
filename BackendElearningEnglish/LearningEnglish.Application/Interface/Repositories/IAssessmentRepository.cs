using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IAssessmentRepository
    {
        // Thêm bài kiểm tra
        Task AddAssessment(Assessment assessment);
        
        // Lấy bài kiểm tra theo ID
        Task<Assessment?> GetAssessmentById(int assessmentId);

        // Lấy tất cả bài kiểm tra của module
        Task<List<Assessment>> GetAssessmentsByModuleId(int moduleId);
        
        // Cập nhật bài kiểm tra
        Task UpdateAssessment(Assessment assessment);

        // Xóa bài kiểm tra
        Task DeleteAssessment(int assessmentId);
        
        // Kiểm tra module tồn tại
        Task<bool> ModuleExists(int moduleId);

        // Kiểm tra giáo viên sở hữu module
        Task<bool> IsTeacherOwnerOfModule(int teacherId, int moduleId);
        
        // Kiểm tra giáo viên sở hữu bài kiểm tra
        Task<bool> IsTeacherOwnerOfAssessmentAsync(int teacherId, int assessmentId);
    }
}
