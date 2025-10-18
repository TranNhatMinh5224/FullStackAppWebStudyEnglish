using CleanDemo.Application.DTOs;
using CleanDemo.Application.Common;

namespace CleanDemo.Application.Interface
{
    public interface IEnrollmentQueryService
    {
        /// <summary>
        /// Lấy danh sách khóa học đã đăng ký của user
        /// </summary>
        Task<ServiceResponse<IEnumerable<CourseResponseDto>>> GetMyEnrolledCoursesAsync(int userId);
    }
}
