using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;

namespace LearningEnglish.Application.Interface
{
    public interface IEnrollmentQueryService
    {
        // Lấy danh sách khóa học đã đăng ký của user với tiến độ - RLS đã filter theo userId
        Task<ServiceResponse<IEnumerable<EnrolledCourseWithProgressDto>>> GetMyEnrolledCoursesAsync(int userId);
        
        // Lấy danh sách khóa học đã đăng ký của user với tiến độ (chỉ phân trang, không filter) - RLS đã filter theo userId
        Task<ServiceResponse<PagedResult<EnrolledCourseWithProgressDto>>> GetMyEnrolledCoursesPagedAsync(int userId, PageRequest request);
    }
}
