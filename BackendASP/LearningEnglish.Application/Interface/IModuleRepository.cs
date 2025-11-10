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

        // Ordering operations
        Task<bool> ReorderModulesAsync(int lessonId, List<(int ModuleId, int NewOrderIndex)> reorderItems);
        Task<int> GetMaxOrderIndexAsync(int lessonId);
        Task<Module?> GetNextModuleAsync(int currentModuleId);
        Task<Module?> GetPreviousModuleAsync(int currentModuleId);

        // Bulk operations
        Task<bool> DeleteMultipleAsync(List<int> moduleIds);
        Task<List<Module>> DuplicateModulesToLessonAsync(List<int> moduleIds, int targetLessonId);

        // Content counting
        Task<Dictionary<int, (int LectureCount, int FlashCardCount, int AssessmentCount)>> GetContentCountsAsync(List<int> moduleIds);

        // Validation
        Task<bool> BelongsToLessonAsync(int moduleId, int lessonId);
        Task<bool> CanUserAccessModuleAsync(int moduleId, int userId);
    }
}
