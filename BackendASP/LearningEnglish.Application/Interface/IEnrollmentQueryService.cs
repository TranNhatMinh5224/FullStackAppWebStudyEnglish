using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface IEnrollmentQueryService
    {
        /// <summary>
        /// Lấy danh sách khóa học đã đăng ký của user
        /// </summary>
        Task<ServiceResponse<IEnumerable<CourseResponseDto>>> GetMyEnrolledCoursesAsync(int userId);
    }
}
