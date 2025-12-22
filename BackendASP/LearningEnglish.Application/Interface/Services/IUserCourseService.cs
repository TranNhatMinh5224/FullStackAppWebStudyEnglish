using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface IUserCourseService
    {
        // GET /api/user/courses/system-courses - Danh sách system courses với enrollment status
        Task<ServiceResponse<IEnumerable<SystemCoursesListResponseDto>>> GetSystemCoursesAsync(int? userId = null);

        // GET /api/user/courses/{courseId} - Chi tiết course với enrollment status
        Task<ServiceResponse<CourseDetailWithEnrollmentDto>> GetCourseByIdAsync(int courseId, int? userId = null);
        // Tìm kiếm khóa học 
        Task<ServiceResponse<IEnumerable<SystemCoursesListResponseDto>>> SearchCoursesAsync(string keyword);
    }
}
