using CleanDemo.Application.DTOs;
using CleanDemo.Application.Common;

namespace CleanDemo.Application.Interface
{
    public interface ICourseService
    {
        // === ADMIN ===
        Task<ServiceResponse<IEnumerable<CourseDto>>> GetAllCoursesAsync();
        Task<ServiceResponse<CourseDetailDto>> GetCourseDetailAsync(int courseId, int? userId = null);

        // === TEACHER ===
        Task<ServiceResponse<CourseDto>> CreateCourseAsync(CreateCourseDto courseDto);
        Task<ServiceResponse<IEnumerable<ListMyCourseTeacherDto>>> GetMyCoursesByTeacherAsync(int teacherId);
        Task<ServiceResponse<bool>> JoinCourseAsTeacherAsync(JoinCourseTeacherDto joinDto, int teacherId);

        // === USER/STUDENT ===
        Task<ServiceResponse<IEnumerable<UserCourseDto>>> GetSystemCoursesAsync(int? userId = null);
        Task<ServiceResponse<IEnumerable<ListMyCourseStudentDto>>> GetMyEnrolledCoursesAsync(int userId);
        Task<ServiceResponse<bool>> EnrollInCourseAsync(EnrollCourseDto enrollDto, int userId);
        Task<ServiceResponse<bool>> UnenrollFromCourseAsync(int courseId, int userId);

        // === UTILITIES ===
        Task<ServiceResponse<bool>> UpdateCourseAsync(int courseId, CreateCourseDto courseDto);
        Task<ServiceResponse<bool>> DeleteCourseAsync(int courseId);
    }
}