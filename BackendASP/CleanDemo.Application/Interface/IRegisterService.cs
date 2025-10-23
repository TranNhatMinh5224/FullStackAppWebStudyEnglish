using CleanDemo.Application.DTOs;
using CleanDemo.Application.Common;

namespace CleanDemo.Application.Interface
{
    public interface IRegisterService
    {
        Task<ServiceResponse<UserDto>> RegisterUserAsync(RegisterUserDto dto);
    }
}
