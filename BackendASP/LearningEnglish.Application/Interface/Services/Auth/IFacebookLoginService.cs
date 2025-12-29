using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface.Auth
{
    public interface IFacebookLoginService
    {
        // Đăng nhập bằng Facebook
        Task<ServiceResponse<AuthResponseDto>> HandleFacebookLoginAsync(FacebookLoginDto facebookLoginDto);
    }
}
