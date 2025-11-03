using CleanDemo.Domain.Entities;

namespace CleanDemo.Application.Interface
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<List<User>> GetAllUsersAsync();
        Task<bool> UpdateRoleTeacher(int userId);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
        Task SaveChangesAsync();
        // Thêm method lấy role
        Task<Role?> GetRoleByNameAsync(string roleName);
        // Thêm Phương thức lấy role theo userId
        Task<bool> GetUserRolesAsync(int userId);


        Task<List<User>> GetAllTeachersAsync();
    }
}
