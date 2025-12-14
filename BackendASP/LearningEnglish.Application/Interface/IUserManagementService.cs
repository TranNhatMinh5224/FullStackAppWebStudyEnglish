using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;

namespace LearningEnglish.Application.Interface
{
    public interface IUserManagementService
    {
        Task<ServiceResponse<UserDto>> GetUserProfileAsync(int userId); // Lấy thông tin người dùng theo userId
        Task<ServiceResponse<UserDto>> UpdateUserProfileAsync(int userId, UpdateUserDto dto); // Cập nhật thông tin người dùng
        Task<ServiceResponse<bool>> UpdateAvatarAsync(int userId, UpdateAvatarDto dto); // Cập nhật avatar người dùng - Trả về true/false

        Task<ServiceResponse<PagedResult<UserDto>>> GetAllUsersPagedAsync(PageRequest request); // Lấy tất cả người dùng với phân trang

        Task<ServiceResponse<BlockAccountResponseDto>> BlockAccountAsync(int userId); // Khóa tài khoản
        Task<ServiceResponse<UnblockAccountResponseDto>> UnblockAccountAsync(int userId); // Mở khóa tài khoản
        Task<ServiceResponse<PagedResult<UserDto>>> GetListBlockedAccountsPagedAsync(PageRequest request); // Lấy danh sách tài khoản bị khóa với phân trang

        Task<ServiceResponse<PagedResult<UserDto>>> GetUsersByCourseIdPagedAsync(int courseId, int userId, string checkRole, PageRequest request);  // Lấy danh sách người dùng theo khóa học với phân trang
        Task<ServiceResponse<StudentDetailInCourseDto>> GetStudentDetailInCourseAsync(int courseId, int studentId, int currentUserId, string currentUserRole); // Lấy thông tin chi tiết học sinh trong course
        Task<ServiceResponse<bool>> AddStudentToCourseByEmailAsync(int courseId, string studentEmail, int currentUserId, string currentUserRole); // Thêm học sinh vào course bằng email
        Task<ServiceResponse<bool>> RemoveStudentFromCourseAsync(int courseId, int studentId, int currentUserId, string currentUserRole); // Xóa học sinh khỏi course

        Task<ServiceResponse<PagedResult<UserDto>>> GetListTeachersPagedAsync(PageRequest request); // Lấy danh sách giáo viên với phân trang
    }
}
