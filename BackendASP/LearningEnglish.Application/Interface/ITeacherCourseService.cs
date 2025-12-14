using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;

namespace LearningEnglish.Application.Interface
{
    public interface ITeacherCourseService
    {
        Task<ServiceResponse<CourseResponseDto>> CreateCourseAsync(TeacherCreateCourseRequestDto requestDto, int teacherId);
        Task<ServiceResponse<CourseResponseDto>> UpdateCourseAsync(int courseId, TeacherUpdateCourseRequestDto requestDto, int teacherId);
        Task<ServiceResponse<PagedResult<CourseResponseDto>>> GetMyCoursesPagedAsync(int teacherId, PageRequest request);
        Task<ServiceResponse<CourseResponseDto>> DeleteCourseAsync(int courseId, int teacherId);
    }
}
