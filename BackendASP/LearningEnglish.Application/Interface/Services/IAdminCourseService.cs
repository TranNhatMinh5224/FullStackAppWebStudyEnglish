using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;

namespace LearningEnglish.Application.Interface
{
    public interface IAdminCourseService
    {
        // Lấy danh sách khóa học (phân trang)
        Task<ServiceResponse<PagedResult<AdminCourseListResponseDto>>> GetAllCoursesPagedAsync(PageRequest request);
        
        // Tạo khóa học
        Task<ServiceResponse<CourseResponseDto>> AdminCreateCourseAsync(AdminCreateCourseRequestDto requestDto);
        
        // Cập nhật khóa học
        Task<ServiceResponse<CourseResponseDto>> AdminUpdateCourseAsync(int courseId, AdminUpdateCourseRequestDto requestDto);
        
        // Xóa khóa học
        Task<ServiceResponse<bool>> DeleteCourseAsync(int courseId);
    }
}
