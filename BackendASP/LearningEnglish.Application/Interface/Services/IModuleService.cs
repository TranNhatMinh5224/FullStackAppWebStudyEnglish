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
        
        // Admin tạo module
        Task<ServiceResponse<ModuleDto>> AdminCreateModule(CreateModuleDto createModuleDto);
        
        // Teacher tạo module
        Task<ServiceResponse<ModuleDto>> TeacherCreateModule(CreateModuleDto createModuleDto, int teacherId);
        
        // Cập nhật module (RLS đã filter theo ownership)
        Task<ServiceResponse<ModuleDto>> UpdateModule(int moduleId, UpdateModuleDto updateModuleDto);
        
        // Xóa module (RLS đã filter theo ownership)
        Task<ServiceResponse<bool>> DeleteModule(int moduleId);

        // Lấy module với tiến độ học
        Task<ServiceResponse<List<ModuleWithProgressDto>>> GetModulesWithProgressAsync(int lessonId, int userId);
        Task<ServiceResponse<ModuleWithProgressDto>> GetModuleWithProgressAsync(int moduleId, int userId);
    }
}
