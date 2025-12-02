using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface IEnrollmentQueryService
    {
        // Lấy danh sách khóa học đã đăng ký của user với tiến độ
        Task<ServiceResponse<IEnumerable<EnrolledCourseWithProgressDto>>> GetMyEnrolledCoursesAsync(int userId);
    }
}
