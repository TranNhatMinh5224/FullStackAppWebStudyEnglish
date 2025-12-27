using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;

namespace LearningEnglish.Application.Interface
{
    public interface ITeacherCourseService
    {
        // Tạo khóa học
        Task<ServiceResponse<CourseResponseDto>> CreateCourseAsync(TeacherCreateCourseRequestDto requestDto, int teacherId);

        // Cập nhật khóa học
        Task<ServiceResponse<CourseResponseDto>> UpdateCourseAsync(int courseId, TeacherUpdateCourseRequestDto requestDto, int teacherId);

        // Lấy danh sách khóa học= 
        Task<ServiceResponse<PagedResult<CourseResponseDto>>> GetMyCoursesPagedAsync(PageRequest request);

        // Xóa khóa học - RLS đã filter theo teacherId
        Task<ServiceResponse<CourseResponseDto>> DeleteCourseAsync(int courseId);

        // Lấy chi tiết khóa học 
        Task<ServiceResponse<TeacherCourseDetailDto>> GetCourseDetailAsync(int courseId);
    }
}
