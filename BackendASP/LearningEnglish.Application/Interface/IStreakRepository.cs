using LearningEnglish.Domain.Entities;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
    public interface IStreakRepository
    {
        Task<Streak?> GetByUserIdAsync(int userId);
        Task<Streak> CreateAsync(Streak streak);
        Task UpdateAsync(Streak streak);
        Task<bool> ExistsAsync(int userId);
    }
}
