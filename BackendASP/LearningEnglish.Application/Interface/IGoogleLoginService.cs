using LearningEnglish.Domain.Entities;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;


namespace LearningEnglish.Application.Interface
{
    public interface IGoogleLoginService
    {
        Task<ServiceResponse<AuthResponseDto>> HandleGoogleLoginAsync(GoogleLoginDto googleLoginDto);
    }
}