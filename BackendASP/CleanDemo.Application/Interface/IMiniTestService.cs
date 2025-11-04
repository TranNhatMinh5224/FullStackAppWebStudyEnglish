using CleanDemo.Application.Common;
using CleanDemo.Application.DTOs;
namespace CleanDemo.Application.Interface
{
    public interface IMiniTestService
    {
        Task<ServiceResponse<MiniTestResponseDto>> AdminAddMiniTest(MiniTestDto dto);
        Task<ServiceResponse<MiniTestResponseDto>> TeacherAddMiniTest(MiniTestDto dto, int teacherId);
    }
}