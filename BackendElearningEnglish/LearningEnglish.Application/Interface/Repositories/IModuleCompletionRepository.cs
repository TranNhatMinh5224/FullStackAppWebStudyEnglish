using LearningEnglish.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
    public interface IModuleCompletionRepository
    {
        // Lấy tiến độ module của user
        Task<ModuleCompletion?> GetByUserAndModuleAsync(int userId, int moduleId);
        
        // Lấy tiến độ nhiều module
        Task<List<ModuleCompletion>> GetByUserAndModuleIdsAsync(int userId, List<int> moduleIds);
        
        // Đếm số module đã hoàn thành
        Task<int> CountCompletedModulesByUserAsync(int userId);
        
        // Thêm tiến độ module
        Task AddAsync(ModuleCompletion moduleCompletion);
        
        // Cập nhật tiến độ module
        Task UpdateAsync(ModuleCompletion moduleCompletion);
    }
}
