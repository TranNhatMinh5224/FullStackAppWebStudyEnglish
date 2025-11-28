using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface IUserManagementService
    {
        Task<ServiceResponse<UserDto>> GetUserProfileAsync(int userId);
        Task<ServiceResponse<UserDto>> UpdateUserProfileAsync(int userId, UpdateUserDto dto);
        Task<ServiceResponse<UserDto>> UpdateAvatarAsync(int userId, UpdateAvatarDto dto);
        Task<ServiceResponse<List<UserDto>>> GetAllUsersAsync();
        Task<ServiceResponse<BlockAccountResponseDto>> BlockAccountAsync(int userId);
        Task<ServiceResponse<UnblockAccountResponseDto>> UnblockAccountAsync(int userId);
        Task<ServiceResponse<List<UserDto>>> GetListBlockedAccountsAsync();

        // Giữ từ feature/LVE-107-GetUserbyCourseId
        // Lấy danh sách người dùng theo id khóa học
        Task<ServiceResponse<List<UserDto>>> GetUsersByCourseIdAsync(int courseId, int userId, string checkRole);

        // Giữ từ dev
        Task<ServiceResponse<List<UserDto>>> GetListTeachersAsync();
        //Lấy danh sách học sinh theo all course
        Task<ServiceResponse<List<StudentsByAllCoursesDto>>> GetStudentsByAllCoursesAsync();
    }
}
