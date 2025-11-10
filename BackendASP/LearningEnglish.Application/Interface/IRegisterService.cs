using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface IRegisterService
    {
        Task<ServiceResponse<UserDto>> RegisterUserAsync(RegisterUserDto dto);
    }
}
