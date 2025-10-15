using CleanDemo.Domain.Entities;

namespace CleanDemo.Application.Interface
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<List<User>> GetAllUsersAsync();
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
        Task SaveChangesAsync();
        // Thêm method lấy role
        Task<Role?> GetRoleByNameAsync(string roleName);
    }
}
