using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;

namespace LearningEnglish.Application.Interface.AdminManagement
{
    public interface IUserManagementService
    {
      
        // Lấy danh sách người dùng phân trang
        Task<ServiceResponse<PagedResult<UserDto>>> GetAllUsersPagedAsync(UserQueryParameters request);

        // Khóa tài khoản
        Task<ServiceResponse<BlockAccountResponseDto>> BlockAccountAsync(int userId);
        
        // Mở khóa tài khoản
        Task<ServiceResponse<UnblockAccountResponseDto>> UnblockAccountAsync(int userId);
        
        // Lấy danh sách tài khoản bị khóa
        Task<ServiceResponse<PagedResult<UserDto>>> GetListBlockedAccountsPagedAsync(UserQueryParameters request);

       
        // Lấy danh sách giáo viên
        Task<ServiceResponse<PagedResult<UserDto>>> GetListTeachersPagedAsync(UserQueryParameters request);
    }
}
