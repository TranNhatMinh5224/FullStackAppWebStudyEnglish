using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IModuleService
    {
        // Lấy thông tin module
        Task<ServiceResponse<ModuleDto>> GetModuleByIdAsync(int moduleId, int? userId = null);
        
        // Lấy danh sách module theo lesson
        Task<ServiceResponse<List<ListModuleDto>>> GetModulesByLessonIdAsync(int lessonId, int? userId = null);
        
        // Tạo module
        Task<ServiceResponse<ModuleDto>> CreateModuleAsync(CreateModuleDto createModuleDto, int createdByUserId, string userRole = "Admin");
        
        // Cập nhật module
        Task<ServiceResponse<ModuleDto>> UpdateModuleAsync(int moduleId, UpdateModuleDto updateModuleDto, int updatedByUserId);
        
        // Xóa module
        Task<ServiceResponse<bool>> DeleteModuleAsync(int moduleId, int deletedByUserId);

        // Cập nhật module có kiểm tra quyền
        Task<ServiceResponse<ModuleDto>> UpdateModuleWithAuthorizationAsync(int moduleId, UpdateModuleDto updateModuleDto, int userId, string userRole);
        
        // Xóa module có kiểm tra quyền
        Task<ServiceResponse<bool>> DeleteModuleWithAuthorizationAsync(int moduleId, int userId, string userRole);

        // Lấy module với tiến độ học
        Task<ServiceResponse<List<ModuleWithProgressDto>>> GetModulesWithProgressAsync(int lessonId, int userId);
        Task<ServiceResponse<ModuleWithProgressDto>> GetModuleWithProgressAsync(int moduleId, int userId);
    }
}
