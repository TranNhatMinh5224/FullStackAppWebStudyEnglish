using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;

namespace LearningEnglish.Application.Interface
{
    public interface IUserCourseService
    {
        // Danh sách system courses 
        Task<ServiceResponse<IEnumerable<SystemCoursesListResponseDto>>> GetSystemCoursesAsync(int? userId = null);

        //Chi tiết course với enrollment status
        Task<ServiceResponse<CourseDetailWithEnrollmentDto>> GetCourseByIdAsync(int courseId, int? userId = null);
        // Tìm kiếm khóa học 
        Task<ServiceResponse<IEnumerable<SystemCoursesListResponseDto>>> SearchCoursesAsync(string keyword);
        // Lấy danh sách khóa học đã đăng ký của user với phân trang
         Task<ServiceResponse<PagedResult<EnrolledCourseWithProgressDto>>> GetMyEnrolledCoursesPagedAsync(int userId, PageRequest request);

         
    }
}
