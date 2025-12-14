using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IModuleService
    {
        // Basic CRUD operations
        Task<ServiceResponse<ModuleDto>> GetModuleByIdAsync(int moduleId, int? userId = null);
        Task<ServiceResponse<List<ListModuleDto>>> GetModulesByLessonIdAsync(int lessonId, int? userId = null);
        Task<ServiceResponse<ModuleDto>> CreateModuleAsync(CreateModuleDto createModuleDto, int createdByUserId, string userRole = "Admin");
        Task<ServiceResponse<ModuleDto>> UpdateModuleAsync(int moduleId, UpdateModuleDto updateModuleDto, int updatedByUserId);
        Task<ServiceResponse<bool>> DeleteModuleAsync(int moduleId, int deletedByUserId);

        // Authorization methods
        Task<ServiceResponse<ModuleDto>> UpdateModuleWithAuthorizationAsync(int moduleId, UpdateModuleDto updateModuleDto, int userId, string userRole);
        Task<ServiceResponse<bool>> DeleteModuleWithAuthorizationAsync(int moduleId, int userId, string userRole);

        // Module with progress (for students)
        Task<ServiceResponse<List<ModuleWithProgressDto>>> GetModulesWithProgressAsync(int lessonId, int userId);
        Task<ServiceResponse<ModuleWithProgressDto>> GetModuleWithProgressAsync(int moduleId, int userId);
    }
}
