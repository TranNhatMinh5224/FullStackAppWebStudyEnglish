using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface.Services.Module
{
    public interface ITeacherModuleService
    {
        // Teacher tạo module
        Task<ServiceResponse<ModuleDto>> TeacherCreateModule(CreateModuleDto dto, int teacherId);
        
        // Teacher lấy module theo ID (own course only)
        Task<ServiceResponse<ModuleDto>> GetModuleById(int moduleId);
        
        // Teacher lấy danh sách module theo lesson (own course only)
        Task<ServiceResponse<List<ListModuleDto>>> GetModulesByLessonId(int lessonId);
        
        // Teacher cập nhật module (own course only)
        Task<ServiceResponse<ModuleDto>> UpdateModule(int moduleId, UpdateModuleDto dto);
        
        // Teacher xóa module (own course only)
        Task<ServiceResponse<bool>> DeleteModule(int moduleId);
    }
}

