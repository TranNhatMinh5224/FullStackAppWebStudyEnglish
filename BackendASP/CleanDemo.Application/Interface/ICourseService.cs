using CleanDemo.Application.DTOs;
using CleanDemo.Application.Common;

namespace CleanDemo.Application.Interface
{
    public interface ICourseService
    {
        // === ADMIN ===
        Task<ServiceResponse<IEnumerable<AdminCourseListResponseDto>>> GetAllCoursesAsync();
        Task<ServiceResponse<CourseResponseDto>> AdminCreateCourseAsync(AdminCreateCourseRequestDto requestDto);
        Task<ServiceResponse<bool>> DeleteCourseAsync(int courseId);
        Task<ServiceResponse<CourseDetailResponseDto>> GetCourseDetailAsync(int courseId, int? userId = null);

        // === TEACHER ===
        Task<ServiceResponse<CourseResponseDto>> CreateCourseAsync(TeacherCreateCourseRequestDto requestDto, int teacherId);
        Task<ServiceResponse<CourseResponseDto>> UpdateCourseAsync(int courseId, TeacherCreateCourseRequestDto requestDto, int teacherId);
        Task<ServiceResponse<IEnumerable<CourseResponseDto>>> GetMyCoursesByTeacherAsync(int teacherId);

        // === USER ===
        Task<ServiceResponse<IEnumerable<UserCourseListResponseDto>>> GetSystemCoursesAsync(int? userId = null);
    }
}