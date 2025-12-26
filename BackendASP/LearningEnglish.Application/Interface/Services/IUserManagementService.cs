using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;

namespace LearningEnglish.Application.Interface
{
    public interface IUserManagementService
    {
        // Lấy thông tin người dùng
        Task<ServiceResponse<UserDto>> GetUserProfileAsync(int userId);
        
        // Cập nhật thông tin người dùng
        Task<ServiceResponse<UserDto>> UpdateUserProfileAsync(int userId, UpdateUserDto dto);
        
        // Cập nhật avatar
        Task<ServiceResponse<bool>> UpdateAvatarAsync(int userId, UpdateAvatarDto dto);

        // Lấy danh sách người dùng phân trang
        Task<ServiceResponse<PagedResult<UserDto>>> GetAllUsersPagedAsync(UserQueryParameters request);

        // Khóa tài khoản
        Task<ServiceResponse<BlockAccountResponseDto>> BlockAccountAsync(int userId);
        
        // Mở khóa tài khoản
        Task<ServiceResponse<UnblockAccountResponseDto>> UnblockAccountAsync(int userId);
        
        // Lấy danh sách tài khoản bị khóa
        Task<ServiceResponse<PagedResult<UserDto>>> GetListBlockedAccountsPagedAsync(UserQueryParameters request);

        // Lấy người dùng theo course
        // RLS tự động filter: Admin xem tất cả, Teacher chỉ xem students trong own courses
        // userId không cần truyền vào service (RLS đã filter), chỉ cần để log ở Controller
        Task<ServiceResponse<PagedResult<UserDto>>> GetUsersByCourseIdPagedAsync(int courseId, PageRequest request);
        
        // Lấy chi tiết học sinh trong course
        // RLS tự động filter: Admin xem tất cả, Teacher chỉ xem students trong own courses
        // currentUserId không cần (RLS đã filter), chỉ cần để log ở Controller
        Task<ServiceResponse<StudentDetailInCourseDto>> GetStudentDetailInCourseAsync(int courseId, int studentId);
        
        // Thêm học sinh vào course
        // RLS tự động filter: Admin thêm vào bất kỳ course nào, Teacher chỉ thêm vào own courses
        Task<ServiceResponse<bool>> AddStudentToCourseByEmailAsync(int courseId, string studentEmail, int currentUserId);
        
        // Xóa học sinh khỏi course
        // RLS tự động filter: Admin xóa bất kỳ student nào, Teacher chỉ xóa students trong own courses
        Task<ServiceResponse<bool>> RemoveStudentFromCourseAsync(int courseId, int studentId, int currentUserId);

        // Lấy danh sách giáo viên
        Task<ServiceResponse<PagedResult<UserDto>>> GetListTeachersPagedAsync(UserQueryParameters request);
    }
}
