using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface.Services.Module
{
    
    public interface IUserModuleService
    {
        // Lấy module với tiến độ học tập
        Task<ServiceResponse<ModuleWithProgressDto>> GetModuleWithProgress(int moduleId, int userId);
        
        // Lấy danh sách module với tiến độ học tập
        Task<ServiceResponse<List<ModuleWithProgressDto>>> GetModulesWithProgress(int lessonId, int userId);
    }
}

