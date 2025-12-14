using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByPhoneNumberAsync(string phoneNumber);
        Task<List<User>> GetAllUsersAsync();

        // Lấy tất cả người dùng với phân trang
        Task<PagedResult<User>> GetAllUsersPagedAsync(PageRequest request);

        // Lấy danh sách người dùng theo khóa học với phân trang
        Task<PagedResult<User>> GetUsersByCourseIdPagedAsync(int courseId, PageRequest request);

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
        Task<PagedResult<User>> GetAllTeachersPagedAsync(PageRequest request); // Lấy danh sách giáo viên với phân trang
        Task<PagedResult<User>> GetListBlockedAccountsPagedAsync(PageRequest request); // Lấy danh sách tài khoản bị khóa với phân trang
    }
}
