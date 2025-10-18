using CleanDemo.Application.DTOs;
using CleanDemo.Application.Common;

namespace CleanDemo.Application.Interface
{
    public interface IEnrollCourseService
    {
        Task<ServiceResponse<bool>> EnrollInCourseAsync(EnrollCourseDto enrollDto, int userId);
        Task<ServiceResponse<bool>> UnenrollFromCourseAsync(int courseId, int userId);
        Task<ServiceResponse<IEnumerable<CourseResponseDto>>> GetMyEnrolledCoursesAsync(int userId);
        Task<ServiceResponse<bool>> JoinCourseAsTeacherAsync(JoinCourseTeacherDto joinDto, int teacherId);
    }
}
