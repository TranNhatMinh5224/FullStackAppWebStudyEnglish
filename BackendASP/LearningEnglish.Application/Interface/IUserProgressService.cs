using LearningEnglish.Application.DTOS;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
    public interface IUserProgressService
    {
        /// <summary>
        /// Get comprehensive progress dashboard for a user
        /// </summary>
        Task<UserProgressDashboardDto> GetUserProgressDashboardAsync(int userId);

        /// <summary>
        /// Get detailed progress for a specific course
        /// </summary>
        Task<CourseProgressDetailDto> GetCourseProgressDetailAsync(int userId, int courseId);

        /// <summary>
        /// Get progress statistics for a user
        /// </summary>
        Task<ProgressStatisticsDto> GetProgressStatisticsAsync(int userId);

        /// <summary>
        /// Update course progress when a lesson is completed
        /// </summary>
        Task UpdateCourseProgressAsync(int userId, int courseId);

        /// <summary>
        /// Update lesson progress when a module is completed
        /// </summary>
        Task UpdateLessonProgressAsync(int userId, int lessonId);

        /// <summary>
        /// Mark module as started
        /// </summary>
        Task StartModuleAsync(int userId, int moduleId);

        /// <summary>
        /// Mark module as completed
        /// </summary>
        Task CompleteModuleAsync(int userId, int moduleId);
    }
}
