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

        // Lấy users theo role
        Task<List<User>> GetUsersByRoleAsync(string roleName);

        // Lấy tất cả user với phân trang
        Task<PagedResult<User>> GetAllUsersPagedAsync(UserQueryParameters request);

        // Lấy user theo khóa học với phân trang
        Task<PagedResult<User>> GetUsersByCourseIdPagedAsync(int courseId, UserQueryParameters request);

        // Lấy user theo khóa học với phân trang cho Teacher (kiểm tra ownership)
        Task<PagedResult<User>> GetUsersByCourseIdPagedForTeacherAsync(int courseId, int teacherId, UserQueryParameters request);

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

        // Kiểm tra user có role Teacher trong database
        Task<bool> HasTeacherRoleAsync(int userId);

        // Lấy tất cả giáo viên
        Task<List<User>> GetAllTeachersAsync();
        
        // Lấy giáo viên với phân trang
        Task<PagedResult<User>> GetAllTeachersPagedAsync(UserQueryParameters request);
        
        // Lấy tài khoản bị khóa với phân trang
        Task<PagedResult<User>> GetListBlockedAccountsPagedAsync(UserQueryParameters request);

        // Lấy dữ liệu chi tiết học sinh trong course cho Teacher (kiểm tra ownership)
        Task<(User? Student, UserCourse? UserCourse, CourseProgress? Progress)> GetStudentDetailDataForTeacherAsync(int courseId, int studentId, int teacherId);
    }
}
