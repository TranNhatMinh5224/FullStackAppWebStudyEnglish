using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;


// Interface for admin course CRUD management cho admin 

namespace LearningEnglish.Application.Interface
{
    public interface IAdminCourseService
    {
        // Lấy danh sách loại khóa học (System/Teacher) - Dùng cho giao diện quản lý Admin để filter
        Task<ServiceResponse<IEnumerable<CourseTypeDto>>> GetCourseTypesAsync();

        // Lấy danh sách khóa học (phân trang và filter) - Sort theo Title mặc định
        Task<ServiceResponse<PagedResult<AdminCourseListResponseDto>>> GetAllCoursesPagedAsync(AdminCourseQueryParameters parameters);

        // Tạo khóa học
        Task<ServiceResponse<CourseResponseDto>> AdminCreateCourseAsync(AdminCreateCourseRequestDto requestDto);

        // Cập nhật khóa học
        Task<ServiceResponse<CourseResponseDto>> AdminUpdateCourseAsync(int courseId, AdminUpdateCourseRequestDto requestDto);

        // Xóa khóa học
        Task<ServiceResponse<bool>> DeleteCourseAsync(int courseId);
    }
}
