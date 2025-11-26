using LearningEnglish.Application.Common;
using LearningEnglish.Application.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Service.ProgressServices
{
    public class LessonProgressService : ILessonProgressService
    {
        private readonly ILessonCompletionRepository _lessonCompletionRepo;
        private readonly IModuleRepository _moduleRepo;
        private readonly IModuleCompletionRepository _moduleCompletionRepo;
        private readonly ILessonRepository _lessonRepo;
        private readonly ICourseProgressService _courseProgressService;
        private readonly ILogger<LessonProgressService> _logger;

        public LessonProgressService(
            ILessonCompletionRepository lessonCompletionRepo,
            IModuleRepository moduleRepo,
            IModuleCompletionRepository moduleCompletionRepo,
            ILessonRepository lessonRepo,
            ICourseProgressService courseProgressService,
            ILogger<LessonProgressService> logger)
        {
            _lessonCompletionRepo = lessonCompletionRepo;
            _moduleRepo = moduleRepo;
            _moduleCompletionRepo = moduleCompletionRepo;
            _lessonRepo = lessonRepo;
            _courseProgressService = courseProgressService;
            _logger = logger;
        }

        public async Task<ServiceResponse<bool>> UpdateLessonProgressAsync(int userId, int lessonId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var lessonCompletion = await _lessonCompletionRepo.GetByUserAndLessonAsync(userId, lessonId);

                if (lessonCompletion == null)
                {
                    response.Success = false;
                    response.Message = $"Lesson completion not found for user {userId} and lesson {lessonId}";
                    _logger.LogWarning("Lesson completion not found for user {UserId} and lesson {LessonId}", userId, lessonId);
                    return response;
                }

                // Get all modules for this lesson
                var allModules = await _moduleRepo.GetByLessonIdAsync(lessonId);
                var moduleIds = allModules.Select(m => m.ModuleId).ToList();

                // Count completed modules
                var moduleCompletions = await _moduleCompletionRepo.GetByUserAndModuleIdsAsync(userId, moduleIds);
                var completedCount = moduleCompletions.Count(mc => mc.IsCompleted);

                lessonCompletion.UpdateModuleProgress(allModules.Count(), completedCount);
                await _lessonCompletionRepo.UpdateAsync(lessonCompletion);

                // If lesson is now completed, update course progress
                if (lessonCompletion.IsCompleted)
                {
                    var courseId = await _lessonRepo.GetCourseIdByLessonIdAsync(lessonId);

                    if (courseId != null)
                    {
                        await _courseProgressService.UpdateCourseProgressAsync(userId, courseId.Value);
                    }
                }

                response.Success = true;
                response.Data = true;
                response.Message = "Lesson progress updated successfully";

                _logger.LogInformation("Updated lesson progress for user {UserId}, lesson {LessonId}: {Progress}%",
                    userId, lessonId, lessonCompletion.CompletionPercentage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lesson progress for user {UserId} and lesson {LessonId}", userId, lessonId);
                response.Success = false;
                response.Message = $"Error updating lesson progress: {ex.Message}";
            }

            return response;
        }
    }
}