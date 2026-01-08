using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface.Services.Module
{
    public interface IAdminModuleService
    {
        // Admin tạo module
        Task<ServiceResponse<ModuleDto>> AdminCreateModule(CreateModuleDto dto);
        
        // Admin lấy module theo ID
        Task<ServiceResponse<ModuleDto>> GetModuleById(int moduleId);
        
        // Admin lấy danh sách module theo lesson
        Task<ServiceResponse<List<ListModuleDto>>> GetModulesByLessonId(int lessonId);
        
        // Admin cập nhật module
        Task<ServiceResponse<ModuleDto>> UpdateModule(int moduleId, UpdateModuleDto dto);
        
        // Admin xóa module
        Task<ServiceResponse<bool>> DeleteModule(int moduleId);
    }
}

