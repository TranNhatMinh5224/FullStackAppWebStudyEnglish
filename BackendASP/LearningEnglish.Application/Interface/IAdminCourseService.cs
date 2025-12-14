using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;

namespace LearningEnglish.Application.Interface
{
    public interface IAdminCourseService
    {
        Task<ServiceResponse<PagedResult<AdminCourseListResponseDto>>> GetAllCoursesPagedAsync(PageRequest request);
        Task<ServiceResponse<CourseResponseDto>> AdminCreateCourseAsync(AdminCreateCourseRequestDto requestDto);
        Task<ServiceResponse<CourseResponseDto>> AdminUpdateCourseAsync(int courseId, AdminUpdateCourseRequestDto requestDto);
        Task<ServiceResponse<bool>> DeleteCourseAsync(int courseId);
    }
}
