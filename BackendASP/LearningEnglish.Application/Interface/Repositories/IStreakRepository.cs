using LearningEnglish.Domain.Entities;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
    public interface IStreakRepository
    {
        // Lấy streak của user
       
        Task<Streak?> GetByUserIdAsync(int userId);
        
        // Tạo streak
        Task<Streak> CreateAsync(Streak streak);
        
        // Cập nhật streak
        Task UpdateAsync(Streak streak);
        
        // Kiểm tra streak tồn tại
      
        Task<bool> ExistsAsync(int userId);
        
        // Lấy danh sách users có streak >= minStreak và LastActivityDate = yesterday (sắp đứt)
        
        Task<List<Streak>> GetUsersAtRiskOfLosingStreakAsync(int minStreak = 3);
    }
}
