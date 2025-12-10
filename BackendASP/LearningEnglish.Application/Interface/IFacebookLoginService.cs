using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IFacebookLoginService
    {
        Task<ServiceResponse<AuthResponseDto>> HandleFacebookLoginAsync(FacebookLoginDto facebookLoginDto);
    }
}
