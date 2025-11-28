using LearningEnglish.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
    public interface ICourseProgressRepository
    {
        Task<CourseProgress?> GetByUserAndCourseAsync(int userId, int courseId);
        Task<List<CourseProgress>> GetByUserIdAsync(int userId);
        Task<int> CountCompletedCoursesByUserAsync(int userId);
        Task AddAsync(CourseProgress courseProgress);
        Task UpdateAsync(CourseProgress courseProgress);
    }

    public interface ILessonCompletionRepository
    {
        Task<LessonCompletion?> GetByUserAndLessonAsync(int userId, int lessonId);
        Task<List<LessonCompletion>> GetByUserIdAsync(int userId);
        Task<List<LessonCompletion>> GetByUserAndLessonIdsAsync(int userId, List<int> lessonIds);
        Task<int> CountCompletedLessonsByUserAsync(int userId);
        Task AddAsync(LessonCompletion lessonCompletion);
        Task UpdateAsync(LessonCompletion lessonCompletion);
    }

    public interface IModuleCompletionRepository
    {
        Task<ModuleCompletion?> GetByUserAndModuleAsync(int userId, int moduleId);
        Task<List<ModuleCompletion>> GetByUserAndModuleIdsAsync(int userId, List<int> moduleIds);
        Task<int> CountCompletedModulesByUserAsync(int userId);
        Task AddAsync(ModuleCompletion moduleCompletion);
        Task UpdateAsync(ModuleCompletion moduleCompletion);
    }
}
