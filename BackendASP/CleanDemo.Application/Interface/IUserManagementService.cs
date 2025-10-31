using CleanDemo.Application.DTOs;
using CleanDemo.Application.Common;

namespace CleanDemo.Application.Interface
{
    public interface IUserManagementService
    {
        Task<ServiceResponse<UserDto>> GetUserProfileAsync(int userId);
        Task<ServiceResponse<UserDto>> UpdateUserProfileAsync(int userId, UpdateUserDto dto);
        Task<ServiceResponse<List<UserDto>>> GetAllUsersAsync();
        Task<ServiceResponse<BlockAccountResponseDto>> BlockAccountAsync(int userId);
        Task<ServiceResponse<UnblockAccountResponseDto>> UnblockAccountAsync(int userId);
    }
}
