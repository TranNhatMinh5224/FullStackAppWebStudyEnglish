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
        Task<ServiceResponse<PagedResult<CourseResponseDto>>> GetMyCoursesPagedAsync(int teacherId, PageRequest request);

        // Xóa khóa học
        Task<ServiceResponse<CourseResponseDto>> DeleteCourseAsync(int courseId, int teacherId);

        // Lấy chi tiết khóa học
        Task<ServiceResponse<TeacherCourseDetailDto>> GetCourseDetailAsync(int courseId, int teacherId);
    }
}
