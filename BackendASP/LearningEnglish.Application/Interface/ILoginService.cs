using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface ILoginService
    {
        Task<ServiceResponse<AuthResponseDto>> LoginUserAsync(LoginUserDto dto);
        Task<ServiceResponse<AuthResponseDto>> LoginByGoogleAsync(GoogleLoginDto dto);

    }
}
