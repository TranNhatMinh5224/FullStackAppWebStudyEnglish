using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOS;

namespace LearningEnglish.Application.Interface
{
    public interface IModuleService
    {
        // Basic CRUD operations
        Task<ServiceResponse<ModuleDto>> GetModuleByIdAsync(int moduleId, int? userId = null);
        Task<ServiceResponse<List<ListModuleDto>>> GetModulesByLessonIdAsync(int lessonId, int? userId = null);
        Task<ServiceResponse<ModuleDto>> CreateModuleAsync(CreateModuleDto createModuleDto, int createdByUserId);
        Task<ServiceResponse<ModuleDto>> UpdateModuleAsync(int moduleId, UpdateModuleDto updateModuleDto, int updatedByUserId);
        Task<ServiceResponse<bool>> DeleteModuleAsync(int moduleId, int deletedByUserId);

        // Module ordering
        Task<ServiceResponse<bool>> ReorderModulesAsync(int lessonId, List<ReorderModuleDto> reorderItems, int userId);

        // Navigation
        Task<ServiceResponse<ModuleDto?>> GetNextModuleAsync(int currentModuleId, int userId);
        Task<ServiceResponse<ModuleDto?>> GetPreviousModuleAsync(int currentModuleId, int userId);

        // Bulk operations
        Task<ServiceResponse<bool>> BulkDeleteModulesAsync(List<int> moduleIds, int userId);
        Task<ServiceResponse<List<ModuleDto>>> DuplicateModulesToLessonAsync(List<int> moduleIds, int targetLessonId, int userId);

        // Module with progress (for students)
        Task<ServiceResponse<List<ModuleWithProgressDto>>> GetModulesWithProgressAsync(int lessonId, int userId);
        Task<ServiceResponse<ModuleWithProgressDto>> GetModuleWithProgressAsync(int moduleId, int userId);

        // Validation helpers
        Task<ServiceResponse<bool>> CanUserAccessModuleAsync(int moduleId, int userId);
        Task<ServiceResponse<bool>> CanUserManageModuleAsync(int moduleId, int userId);
    }
}
