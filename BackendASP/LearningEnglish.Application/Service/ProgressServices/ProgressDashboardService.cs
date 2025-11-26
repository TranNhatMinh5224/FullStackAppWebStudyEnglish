using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Service.ProgressServices
{
    public class ProgressDashboardService : IProgressDashboardService
    {
        private readonly ICourseProgressRepository _courseProgressRepo;
        private readonly ILessonCompletionRepository _lessonCompletionRepo;
        private readonly IModuleCompletionRepository _moduleCompletionRepo;
        private readonly IUserRepository _userRepo;
        private readonly ICourseProgressService _courseProgressService;
        private readonly ILogger<ProgressDashboardService> _logger;

        public ProgressDashboardService(
            ICourseProgressRepository courseProgressRepo,
            ILessonCompletionRepository lessonCompletionRepo,
            IModuleCompletionRepository moduleCompletionRepo,
            IUserRepository userRepo,
            ICourseProgressService courseProgressService,
            ILogger<ProgressDashboardService> logger)
        {
            _courseProgressRepo = courseProgressRepo;
            _lessonCompletionRepo = lessonCompletionRepo;
            _moduleCompletionRepo = moduleCompletionRepo;
            _userRepo = userRepo;
            _courseProgressService = courseProgressService;
            _logger = logger;
        }

        public async Task<ServiceResponse<UserProgressDashboardDto>> GetUserProgressDashboardAsync(int userId)
        {
            var response = new ServiceResponse<UserProgressDashboardDto>();

            try
            {
                var user = await _userRepo.GetByIdAsync(userId);

                if (user == null)
                {
                    response.Success = false;
                    response.Message = $"User with ID {userId} not found";
                    return response;
                }

                // Get all enrolled courses with progress
                var courseProgresses = await _courseProgressRepo.GetByUserIdAsync(userId);

                var coursesDetail = new List<CourseProgressDetailDto>();

                foreach (var courseProgress in courseProgresses)
                {
                    var courseDetailResponse = await _courseProgressService.GetCourseProgressDetailAsync(userId, courseProgress.CourseId);
                    if (courseDetailResponse.Success && courseDetailResponse.Data != null)
                    {
                        coursesDetail.Add(courseDetailResponse.Data);
                    }
                }

                // Get statistics
                var statisticsResponse = await GetProgressStatisticsAsync(userId);
                var statistics = statisticsResponse.Success && statisticsResponse.Data != null
                    ? statisticsResponse.Data
                    : new ProgressStatisticsDto();

                var dashboard = new UserProgressDashboardDto
                {
                    UserId = userId,
                    UserName = $"{user.FirstName} {user.LastName}".Trim(),
                    Courses = coursesDetail,
                    Statistics = statistics
                };

                response.Success = true;
                response.Data = dashboard;
                response.Message = "User progress dashboard retrieved successfully";

                _logger.LogInformation("Retrieved user progress dashboard for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user progress dashboard for user {UserId}", userId);
                response.Success = false;
                response.Message = $"Error retrieving progress dashboard: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<ProgressStatisticsDto>> GetProgressStatisticsAsync(int userId)
        {
            var response = new ServiceResponse<ProgressStatisticsDto>();

            try
            {
                // Course statistics
                var courseProgresses = await _courseProgressRepo.GetByUserIdAsync(userId);
                var totalCoursesEnrolled = courseProgresses.Count;
                var totalCoursesCompleted = await _courseProgressRepo.CountCompletedCoursesByUserAsync(userId);

                // Lesson statistics
                var totalLessonsCompleted = await _lessonCompletionRepo.CountCompletedLessonsByUserAsync(userId);

                // Module statistics
                var totalModulesCompleted = await _moduleCompletionRepo.CountCompletedModulesByUserAsync(userId);

                // TODO: Quiz, FlashCard, Pronunciation statistics
                // These require additional repository methods or refactoring
                // For now, returning zero values

                var statistics = new ProgressStatisticsDto
                {
                    TotalCoursesEnrolled = totalCoursesEnrolled,
                    TotalCoursesCompleted = totalCoursesCompleted,
                    TotalLessonsCompleted = totalLessonsCompleted,
                    TotalModulesCompleted = totalModulesCompleted,
                    TotalQuizzesTaken = 0, // TODO: Implement when quiz repository has proper methods
                    AverageQuizScore = 0,
                    TotalFlashcardsReviewed = 0, // TODO: Implement
                    CurrentReviewStreak = 0,
                    TotalPronunciationAttempts = 0, // TODO: Implement
                    AveragePronunciationScore = 0
                };

                response.Success = true;
                response.Data = statistics;
                response.Message = "Progress statistics retrieved successfully";

                _logger.LogInformation("Retrieved progress statistics for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting progress statistics for user {UserId}", userId);
                response.Success = false;
                response.Message = $"Error retrieving progress statistics: {ex.Message}";
            }

            return response;
        }
    }
}