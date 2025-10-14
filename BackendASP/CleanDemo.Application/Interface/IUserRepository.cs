using CleanDemo.Domain.Entities;

namespace CleanDemo.Application.Interface
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id); // Sửa từ GetUserByIdAsync thành GetByIdAsync
        Task<User?> GetUserByEmailAsync(string email);
        Task<List<User>> GetAllUsersAsync();
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
        Task SaveChangesAsync();
    }
}
