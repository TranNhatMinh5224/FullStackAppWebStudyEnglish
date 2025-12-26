using LearningEnglish.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
    public interface IModuleCompletionRepository
    {
        // Lấy tiến độ module của user
        // RLS: User chỉ xem completions của chính mình, Teacher xem completions của students trong own courses, Admin xem tất cả
        // userId parameter: Defense in depth (RLS + userId filter) + Teacher có thể query completions của students
        Task<ModuleCompletion?> GetByUserAndModuleAsync(int userId, int moduleId);
        
        // Lấy tiến độ nhiều module
        // RLS: User chỉ xem completions của chính mình, Admin xem tất cả (có permission)
        // userId parameter: Defense in depth (RLS + userId filter)
        Task<List<ModuleCompletion>> GetByUserAndModuleIdsAsync(int userId, List<int> moduleIds);
        
        // Đếm số module đã hoàn thành
        // RLS: User chỉ đếm completions của chính mình, Admin đếm tất cả (có permission)
        // userId parameter: Defense in depth (RLS + userId filter)
        Task<int> CountCompletedModulesByUserAsync(int userId);
        
        // Thêm tiến độ module
        Task AddAsync(ModuleCompletion moduleCompletion);
        
        // Cập nhật tiến độ module
        Task UpdateAsync(ModuleCompletion moduleCompletion);
    }
}
