using CleanDemo.Application.Common;
using CleanDemo.Application.DTOs;
namespace CleanDemo.Application.Interface
{
    public interface IMiniTestService
    {
        Task<ServiceResponse<MiniTestResponseDto>> AdminAddMiniTest(MiniTestDto dto);
        Task<ServiceResponse<MiniTestResponseDto>> TeacherAddMiniTest(MiniTestDto dto, int teacherId);
        Task<ServiceResponse<List<MiniTestResponseDto>>> GetAllMiniTests(int lessonId);
        Task<ServiceResponse<MiniTestResponseDto>> AdminUpdateMiniTest(int miniTestId, UpdateMiniTestDto dto);
        Task<ServiceResponse<MiniTestResponseDto>> TeacherUpdateMiniTest(int miniTestId, UpdateMiniTestDto dto, int teacherId);
    }
}
