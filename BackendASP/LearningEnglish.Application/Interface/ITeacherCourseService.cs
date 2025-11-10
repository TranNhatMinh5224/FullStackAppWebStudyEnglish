using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface ITeacherCourseService
    {
        Task<ServiceResponse<CourseResponseDto>> CreateCourseAsync(TeacherCreateCourseRequestDto requestDto, int teacherId);
        Task<ServiceResponse<CourseResponseDto>> UpdateCourseAsync(int courseId, TeacherUpdateCourseRequestDto requestDto, int teacherId);
        Task<ServiceResponse<IEnumerable<CourseResponseDto>>> GetMyCoursesByTeacherAsync(int teacherId);
    }
}
