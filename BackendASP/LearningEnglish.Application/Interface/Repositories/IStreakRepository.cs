using LearningEnglish.Domain.Entities;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
    public interface IStreakRepository
    {
        // Lấy streak của user
        // RLS: User chỉ xem streak của chính mình, Admin xem tất cả (có permission)
        // userId parameter: Defense in depth (RLS + userId filter) + Admin có thể query streak của user khác
        Task<Streak?> GetByUserIdAsync(int userId);
        
        // Tạo streak
        Task<Streak> CreateAsync(Streak streak);
        
        // Cập nhật streak
        Task UpdateAsync(Streak streak);
        
        // Kiểm tra streak tồn tại
        // RLS: User chỉ check streak của chính mình, Admin check tất cả (có permission)
        // userId parameter: Defense in depth (RLS + userId filter)
        Task<bool> ExistsAsync(int userId);
        
        // Lấy danh sách users có streak >= minStreak và LastActivityDate = yesterday (sắp đứt)
        // RLS: Chỉ Admin có permission Admin.User.Manage mới xem được
        Task<List<Streak>> GetUsersAtRiskOfLosingStreakAsync(int minStreak = 3);
    }
}
