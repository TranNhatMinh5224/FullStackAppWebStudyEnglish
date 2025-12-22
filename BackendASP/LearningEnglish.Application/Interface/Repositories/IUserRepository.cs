using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IUserRepository
    {
        // Lấy user theo ID
        Task<User?> GetByIdAsync(int id);
        
        // Lấy user theo email
        Task<User?> GetUserByEmailAsync(string email);
        
        // Lấy user theo số điện thoại
        Task<User?> GetUserByPhoneNumberAsync(string phoneNumber);
        
        // Lấy tất cả user
        Task<List<User>> GetAllUsersAsync();

        // Lấy tất cả user với phân trang
        Task<PagedResult<User>> GetAllUsersPagedAsync(PageRequest request);

        // Lấy user theo khóa học với phân trang
        Task<PagedResult<User>> GetUsersByCourseIdPagedAsync(int courseId, PageRequest request);

        // Cập nhật quyền giáo viên
        Task<bool> UpdateRoleTeacher(int userId);
        
        // Thêm user
        Task AddUserAsync(User user);
        
        // Cập nhật user
        Task UpdateUserAsync(User user);
        
        // Xóa user
        Task DeleteUserAsync(int id);
        
        // Lưu thay đổi
        Task SaveChangesAsync();
        
        // Lấy role theo tên
        Task<Role?> GetRoleByNameAsync(string roleName);
        
        // Lấy role của user
        Task<bool> GetUserRolesAsync(int userId);

        // Lấy tất cả giáo viên
        Task<List<User>> GetAllTeachersAsync();
        
        // Lấy giáo viên với phân trang
        Task<PagedResult<User>> GetAllTeachersPagedAsync(PageRequest request);
        
        // Lấy tài khoản bị khóa với phân trang
        Task<PagedResult<User>> GetListBlockedAccountsPagedAsync(PageRequest request);
    }
}
