using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOS;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Service
{
    public class UserProgressService : IUserProgressService
    {
        private readonly ICourseProgressRepository _courseProgressRepo;
        private readonly ILessonCompletionRepository _lessonCompletionRepo;
        private readonly IModuleCompletionRepository _moduleCompletionRepo;
        private readonly IUserRepository _userRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly ILessonRepository _lessonRepo;
        private readonly IModuleRepository _moduleRepo;
        private readonly IQuizAttemptRepository _quizAttemptRepo;
        private readonly IFlashCardReviewRepository _flashCardReviewRepo;
        private readonly IPronunciationAssessmentRepository _pronunciationRepo;
        private readonly ILogger<UserProgressService> _logger;

        public UserProgressService(
            ICourseProgressRepository courseProgressRepo,
            ILessonCompletionRepository lessonCompletionRepo,
            IModuleCompletionRepository moduleCompletionRepo,
            IUserRepository userRepo,
            ICourseRepository courseRepo,
            ILessonRepository lessonRepo,
            IModuleRepository moduleRepo,
            IQuizAttemptRepository quizAttemptRepo,
            IFlashCardReviewRepository flashCardReviewRepo,
            IPronunciationAssessmentRepository pronunciationRepo,
            ILogger<UserProgressService> logger)
        {
            _courseProgressRepo = courseProgressRepo;
            _lessonCompletionRepo = lessonCompletionRepo;
            _moduleCompletionRepo = moduleCompletionRepo;
            _userRepo = userRepo;
            _courseRepo = courseRepo;
            _lessonRepo = lessonRepo;
            _moduleRepo = moduleRepo;
            _quizAttemptRepo = quizAttemptRepo;
            _flashCardReviewRepo = flashCardReviewRepo;
            _pronunciationRepo = pronunciationRepo;
            _logger = logger;
        }

        public async Task<UserProgressDashboardDto> GetUserProgressDashboardAsync(int userId)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(userId);

                if (user == null)
                {
                    throw new Exception($"User with ID {userId} not found");
                }

                // Get all enrolled courses with progress
                var courseProgresses = await _courseProgressRepo.GetByUserIdAsync(userId);

                var coursesDetail = new List<CourseProgressDetailDto>();

                foreach (var courseProgress in courseProgresses)
                {
                    var courseDetail = await GetCourseProgressDetailAsync(userId, courseProgress.CourseId);
                    coursesDetail.Add(courseDetail);
                }

                // Get statistics
                var statistics = await GetProgressStatisticsAsync(userId);

                return new UserProgressDashboardDto
                {
                    UserId = userId,
                    UserName = $"{user.FirstName} {user.LastName}".Trim(),
                    Courses = coursesDetail,
                    Statistics = statistics
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user progress dashboard for user {UserId}", userId);
                throw;
            }
        }

        public async Task<CourseProgressDetailDto> GetCourseProgressDetailAsync(int userId, int courseId)
        {
            try
            {
                var courseProgress = await _courseProgressRepo.GetByUserAndCourseAsync(userId, courseId);

                if (courseProgress == null)
                {
                    throw new Exception($"Course progress not found for user {userId} and course {courseId}");
                }

                var course = courseProgress.Course;

                // Get all lesson completions for this course
                var lessonIds = course.Lessons.Select(l => l.LessonId).ToList();
                var lessonCompletions = await _lessonCompletionRepo.GetByUserAndLessonIdsAsync(userId, lessonIds);

                var lessonsDetail = new List<LessonProgressDetailDto>();

                foreach (var lesson in course.Lessons.OrderBy(l => l.OrderIndex))
                {
                    var lessonCompletion = lessonCompletions
                        .FirstOrDefault(lc => lc.LessonId == lesson.LessonId);

                    // Get modules for this lesson
                    var modules = await _moduleRepo.GetByLessonIdAsync(lesson.LessonId);

                    var moduleDetails = new List<ModuleProgressDetailDto>();

                    if (lessonCompletion != null)
                    {
                        var moduleIds = modules.Select(m => m.ModuleId).ToList();
                        var moduleCompletions = await _moduleCompletionRepo.GetByUserAndModuleIdsAsync(userId, moduleIds);

                        foreach (var module in modules)
                        {
                            var moduleCompletion = moduleCompletions
                                .FirstOrDefault(mc => mc.ModuleId == module.ModuleId);

                            moduleDetails.Add(new ModuleProgressDetailDto
                            {
                                ModuleId = module.ModuleId,
                                ModuleName = module.Name,
                                ModuleType = module.ContentType.ToString(),
                                OrderIndex = module.OrderIndex,
                                IsCompleted = moduleCompletion?.IsCompleted ?? false,
                                ProgressPercentage = moduleCompletion?.ProgressPercentage ?? 0,
                                StartedAt = moduleCompletion?.StartedAt,
                                CompletedAt = moduleCompletion?.CompletedAt
                            });
                        }
                    }
                    else
                    {
                        // No progress yet, return modules with 0% progress
                        foreach (var module in modules)
                        {
                            moduleDetails.Add(new ModuleProgressDetailDto
                            {
                                ModuleId = module.ModuleId,
                                ModuleName = module.Name,
                                ModuleType = module.ContentType.ToString(),
                                OrderIndex = module.OrderIndex,
                                IsCompleted = false,
                                ProgressPercentage = 0,
                                StartedAt = null,
                                CompletedAt = null
                            });
                        }
                    }

                    lessonsDetail.Add(new LessonProgressDetailDto
                    {
                        LessonId = lesson.LessonId,
                        LessonName = lesson.Title,
                        OrderIndex = lesson.OrderIndex,
                        CompletionPercentage = (decimal)(lessonCompletion?.CompletionPercentage ?? 0),
                        CompletedModules = lessonCompletion?.CompletedModules ?? 0,
                        TotalModules = lessonCompletion?.TotalModules ?? modules.Count(),
                        VideoProgressPercentage = (decimal)(lessonCompletion?.VideoProgressPercentage ?? 0),
                        IsCompleted = lessonCompletion?.IsCompleted ?? false,
                        StartedAt = lessonCompletion?.StartedAt,
                        CompletedAt = lessonCompletion?.CompletedAt,
                        Modules = moduleDetails
                    });
                }

                return new CourseProgressDetailDto
                {
                    CourseId = course.CourseId,
                    CourseName = course.Title,
                    CourseDescription = course.Description,
                    ProgressPercentage = courseProgress.ProgressPercentage,
                    CompletedLessons = courseProgress.CompletedLessons,
                    TotalLessons = courseProgress.TotalLessons,
                    IsCompleted = courseProgress.IsCompleted,
                    EnrolledAt = courseProgress.EnrolledAt,
                    CompletedAt = courseProgress.CompletedAt,
                    LastAccessedAt = null,
                    Lessons = lessonsDetail
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course progress detail for user {UserId} and course {CourseId}", userId, courseId);
                throw;
            }
        }

        public async Task<ProgressStatisticsDto> GetProgressStatisticsAsync(int userId)
        {
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

                return new ProgressStatisticsDto
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting progress statistics for user {UserId}", userId);
                throw;
            }
        }

        public async Task UpdateCourseProgressAsync(int userId, int courseId)
        {
            try
            {
                var courseProgress = await _courseProgressRepo.GetByUserAndCourseAsync(userId, courseId);

                if (courseProgress == null)
                {
                    _logger.LogWarning("Course progress not found for user {UserId} and course {CourseId}", userId, courseId);
                    return;
                }

                courseProgress.UpdateProgress(courseProgress.TotalLessons, courseProgress.CompletedLessons);
                await _courseProgressRepo.UpdateAsync(courseProgress);

                _logger.LogInformation("Updated course progress for user {UserId}, course {CourseId}: {Progress}%", 
                    userId, courseId, courseProgress.ProgressPercentage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course progress for user {UserId} and course {CourseId}", userId, courseId);
                throw;
            }
        }

        public async Task UpdateLessonProgressAsync(int userId, int lessonId)
        {
            try
            {
                var lessonCompletion = await _lessonCompletionRepo.GetByUserAndLessonAsync(userId, lessonId);

                if (lessonCompletion == null)
                {
                    _logger.LogWarning("Lesson completion not found for user {UserId} and lesson {LessonId}", userId, lessonId);
                    return;
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
                    var lesson = await _lessonRepo.GetLessonById(lessonId);

                    if (lesson != null)
                    {
                        await UpdateCourseProgressAsync(userId, lesson.CourseId);
                    }
                }

                _logger.LogInformation("Updated lesson progress for user {UserId}, lesson {LessonId}: {Progress}%", 
                    userId, lessonId, lessonCompletion.CompletionPercentage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lesson progress for user {UserId} and lesson {LessonId}", userId, lessonId);
                throw;
            }
        }

        public async Task StartModuleAsync(int userId, int moduleId)
        {
            try
            {
                var existingCompletion = await _moduleCompletionRepo.GetByUserAndModuleAsync(userId, moduleId);

                if (existingCompletion != null)
                {
                    _logger.LogInformation("Module {ModuleId} already started for user {UserId}", moduleId, userId);
                    return;
                }

                var module = await _moduleRepo.GetByIdAsync(moduleId);

                if (module == null)
                {
                    throw new Exception($"Module with ID {moduleId} not found");
                }

                // Create ModuleCompletion
                var moduleCompletion = new ModuleCompletion
                {
                    UserId = userId,
                    ModuleId = moduleId
                };
                moduleCompletion.MarkAsStarted();

                await _moduleCompletionRepo.AddAsync(moduleCompletion);

                // Create LessonCompletion if not exists
                var lessonCompletion = await _lessonCompletionRepo.GetByUserAndLessonAsync(userId, module.LessonId);

                if (lessonCompletion == null)
                {
                    var allModules = await _moduleRepo.GetByLessonIdAsync(module.LessonId);

                    lessonCompletion = new LessonCompletion
                    {
                        UserId = userId,
                        LessonId = module.LessonId,
                        TotalModules = allModules.Count(),
                        StartedAt = DateTime.UtcNow
                    };

                    await _lessonCompletionRepo.AddAsync(lessonCompletion);
                }

                _logger.LogInformation("Started module {ModuleId} for user {UserId}", moduleId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting module {ModuleId} for user {UserId}", moduleId, userId);
                throw;
            }
        }

        public async Task CompleteModuleAsync(int userId, int moduleId)
        {
            try
            {
                var moduleCompletion = await _moduleCompletionRepo.GetByUserAndModuleAsync(userId, moduleId);

                if (moduleCompletion == null)
                {
                    // If not started, start and complete it
                    await StartModuleAsync(userId, moduleId);
                    moduleCompletion = await _moduleCompletionRepo.GetByUserAndModuleAsync(userId, moduleId);
                }

                if (moduleCompletion != null && !moduleCompletion.IsCompleted)
                {
                    moduleCompletion.MarkAsCompleted();
                    await _moduleCompletionRepo.UpdateAsync(moduleCompletion);

                    // Update lesson progress
                    var module = await _moduleRepo.GetByIdAsync(moduleId);

                    if (module != null)
                    {
                        await UpdateLessonProgressAsync(userId, module.LessonId);
                    }

                    _logger.LogInformation("Completed module {ModuleId} for user {UserId}", moduleId, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing module {ModuleId} for user {UserId}", moduleId, userId);
                throw;
            }
        }

        private int CalculateReviewStreak(List<FlashCardReview> reviews)
        {
            if (!reviews.Any())
                return 0;

            var sortedReviews = reviews
                .OrderByDescending(r => r.ReviewedAt)
                .Select(r => r.ReviewedAt.Date)
                .Distinct()
                .ToList();

            int streak = 0;
            var currentDate = DateTime.UtcNow.Date;

            foreach (var reviewDate in sortedReviews)
            {
                if (reviewDate == currentDate.AddDays(-streak))
                {
                    streak++;
                }
                else
                {
                    break;
                }
            }

            return streak;
        }
    }
}
