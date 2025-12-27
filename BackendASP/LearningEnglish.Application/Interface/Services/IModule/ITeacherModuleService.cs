using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface.Services.Module
{
    public interface ITeacherModuleService
    {
        // Teacher tạo module
        Task<ServiceResponse<ModuleDto>> TeacherCreateModule(CreateModuleDto dto, int teacherId);
        
        // Teacher lấy module theo ID (own course only)
        Task<ServiceResponse<ModuleDto>> GetModuleById(int moduleId, int teacherId);
        
        // Teacher lấy danh sách module theo lesson (own course only)
        Task<ServiceResponse<List<ListModuleDto>>> GetModulesByLessonId(int lessonId, int teacherId);
        
        // Teacher cập nhật module (own course only)
        Task<ServiceResponse<ModuleDto>> UpdateModule(int moduleId, UpdateModuleDto dto, int teacherId);
        
        // Teacher xóa module (own course only)
        Task<ServiceResponse<bool>> DeleteModule(int moduleId, int teacherId);
    }
}

