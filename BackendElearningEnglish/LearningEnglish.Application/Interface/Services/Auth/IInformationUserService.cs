using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;

namespace LearningEnglish.Application.Interface.Auth
{
    public interface IInformationUserService
    {
        // Lấy thông tin người dùng
        Task<ServiceResponse<UserDto>> GetUserProfileAsync(int userId);
        
        // Cập nhật thông tin người dùng
        Task<ServiceResponse<UserDto>> UpdateUserProfileAsync(int userId, UpdateUserDto dto);
        
        // Cập nhật avatar
        Task<ServiceResponse<bool>> UpdateAvatarAsync(int userId, UpdateAvatarDto dto);


    }
}
