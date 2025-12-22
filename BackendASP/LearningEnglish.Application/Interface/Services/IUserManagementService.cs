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
        Task<ServiceResponse<PagedResult<UserDto>>> GetAllUsersPagedAsync(PageRequest request);

        // Khóa tài khoản
        Task<ServiceResponse<BlockAccountResponseDto>> BlockAccountAsync(int userId);
        
        // Mở khóa tài khoản
        Task<ServiceResponse<UnblockAccountResponseDto>> UnblockAccountAsync(int userId);
        
        // Lấy danh sách tài khoản bị khóa
        Task<ServiceResponse<PagedResult<UserDto>>> GetListBlockedAccountsPagedAsync(PageRequest request);

        // Lấy người dùng theo course
        Task<ServiceResponse<PagedResult<UserDto>>> GetUsersByCourseIdPagedAsync(int courseId, int userId, string checkRole, PageRequest request);
        
        // Lấy chi tiết học sinh trong course
        Task<ServiceResponse<StudentDetailInCourseDto>> GetStudentDetailInCourseAsync(int courseId, int studentId, int currentUserId, string currentUserRole);
        
        // Thêm học sinh vào course
        Task<ServiceResponse<bool>> AddStudentToCourseByEmailAsync(int courseId, string studentEmail, int currentUserId, string currentUserRole);
        
        // Xóa học sinh khỏi course
        Task<ServiceResponse<bool>> RemoveStudentFromCourseAsync(int courseId, int studentId, int currentUserId, string currentUserRole);

        // Lấy danh sách giáo viên
        Task<ServiceResponse<PagedResult<UserDto>>> GetListTeachersPagedAsync(PageRequest request);
    }
}
