using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface IUserCourseService
    {
        Task<ServiceResponse<IEnumerable<UserCourseListResponseDto>>> GetSystemCoursesAsync(int? userId = null);
    }
}
