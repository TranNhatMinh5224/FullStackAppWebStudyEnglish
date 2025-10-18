using CleanDemo.Application.DTOs;
using CleanDemo.Application.Common;

namespace CleanDemo.Application.Interface
{
    public interface IAuthenticationService
    {
        Task<ServiceResponse<UserDto>> RegisterUserAsync(RegisterUserDto dto);
        Task<ServiceResponse<AuthResponseDto>> LoginUserAsync(LoginUserDto dto);
        Task<ServiceResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken);
        Task<ServiceResponse<bool>> LogoutAsync(string refreshToken);
    }
}
