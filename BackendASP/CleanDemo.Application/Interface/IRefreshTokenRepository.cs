using CleanDemo.Domain.Entities;

namespace CleanDemo.Application.Interface
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<List<RefreshToken>> GetTokensByUserIdAsync(int userId);
        Task AddAsync(RefreshToken refreshToken);
        Task UpdateAsync(RefreshToken refreshToken);
        Task DeleteAsync(string token);
        Task SaveChangesAsync();
    }
}
