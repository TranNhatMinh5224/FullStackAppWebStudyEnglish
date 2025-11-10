using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface IAdminCourseService
    {
        Task<ServiceResponse<IEnumerable<AdminCourseListResponseDto>>> GetAllCoursesAsync();
        Task<ServiceResponse<CourseResponseDto>> AdminCreateCourseAsync(AdminCreateCourseRequestDto requestDto);
        Task<ServiceResponse<CourseResponseDto>> AdminUpdateCourseAsync(int courseId, AdminUpdateCourseRequestDto requestDto);
        Task<ServiceResponse<bool>> DeleteCourseAsync(int courseId);
    }
}
