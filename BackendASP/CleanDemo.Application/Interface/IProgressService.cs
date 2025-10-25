using CleanDemo.Application.DTOs;
using CleanDemo.Application.Common;

namespace CleanDemo.Application.Interface
{
    public interface IProgressService
    {
        Task<ServiceResponse<bool>> UpdateLessonProgress(int userId, int lessonId, double completion);
        Task<ServiceResponse<bool>> CompleteLessonAsync(int userId, int lessonId);
        Task<ServiceResponse<CourseProgressDto>> GetCourseProgressAsync(int userId, int courseId);
        Task<ServiceResponse<List<CourseProgressDto>>> GetAllUserProgressAsync(int userId);
    }
}
