using CleanDemo.Application.DTOs;
using CleanDemo.Application.Common;

namespace CleanDemo.Application.Interface
{
    public interface IAdminCourseService
    {
        Task<ServiceResponse<IEnumerable<AdminCourseListResponseDto>>> GetAllCoursesAsync();
        Task<ServiceResponse<CourseResponseDto>> AdminCreateCourseAsync(AdminCreateCourseRequestDto requestDto);
        Task<ServiceResponse<bool>> DeleteCourseAsync(int courseId);
    }
}
