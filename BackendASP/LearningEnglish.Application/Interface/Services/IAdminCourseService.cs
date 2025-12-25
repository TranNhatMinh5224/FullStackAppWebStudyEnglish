using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;

namespace LearningEnglish.Application.Interface
{
    public interface IAdminCourseService
    {
        // Lấy danh sách loại khóa học
        List<CourseTypeDto> GetCourseTypes();
        
        // Lấy danh sách khóa học (phân trang và filter)
        Task<ServiceResponse<PagedResult<AdminCourseListResponseDto>>> GetAllCoursesPagedAsync(CourseQueryParameters parameters);
        
        // Tạo khóa học
        Task<ServiceResponse<CourseResponseDto>> AdminCreateCourseAsync(AdminCreateCourseRequestDto requestDto);
        
        // Cập nhật khóa học
        Task<ServiceResponse<CourseResponseDto>> AdminUpdateCourseAsync(int courseId, AdminUpdateCourseRequestDto requestDto);
        
        // Xóa khóa học
        Task<ServiceResponse<bool>> DeleteCourseAsync(int courseId);
    }
}
