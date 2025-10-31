using CleanDemo.Application.DTOs;
using CleanDemo.Application.Common;

namespace CleanDemo.Application.Interface
{
    public interface ILoginService
    {
        Task<ServiceResponse<AuthResponseDto>> LoginUserAsync(LoginUserDto dto);




    }
}
