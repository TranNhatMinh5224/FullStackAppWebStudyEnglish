using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IModuleRepository
    {
        // Basic CRUD operations
        Task<Module?> GetByIdAsync(int moduleId);
        Task<Module?> GetByIdWithDetailsAsync(int moduleId);
        Task<List<Module>> GetByLessonIdAsync(int lessonId);
        Task<List<Module>> GetByLessonIdWithDetailsAsync(int lessonId);
        Task<Module> CreateAsync(Module module);
        Task<Module> UpdateAsync(Module module);
        Task<bool> DeleteAsync(int moduleId);
        Task<bool> ExistsAsync(int moduleId);

        // Helper operations
        Task<int> GetMaxOrderIndexAsync(int lessonId);
        Task<int?> GetLessonIdByModuleIdAsync(int moduleId);
        
        // Authorization helpers
        Task<Module?> GetModuleWithCourseAsync(int moduleId);
    }
}
