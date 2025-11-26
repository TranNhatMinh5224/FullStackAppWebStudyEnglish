using AutoMapper;
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
    public class CourseProgressService : ICourseProgressService
    {
        private readonly ICourseProgressRepository _courseProgressRepo;
        private readonly ILessonCompletionRepository _lessonCompletionRepo;
        private readonly IModuleCompletionRepository _moduleCompletionRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly ILessonRepository _lessonRepo;
        private readonly IModuleRepository _moduleRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<CourseProgressService> _logger;

        public CourseProgressService(
            ICourseProgressRepository courseProgressRepo,
            ILessonCompletionRepository lessonCompletionRepo,
            IModuleCompletionRepository moduleCompletionRepo,
            ICourseRepository courseRepo,
            ILessonRepository lessonRepo,
            IModuleRepository moduleRepo,
            IMapper mapper,
            ILogger<CourseProgressService> logger)
        {
            _courseProgressRepo = courseProgressRepo;
            _lessonCompletionRepo = lessonCompletionRepo;
            _moduleCompletionRepo = moduleCompletionRepo;
            _courseRepo = courseRepo;
            _lessonRepo = lessonRepo;
            _moduleRepo = moduleRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ServiceResponse<CourseProgressDetailDto>> GetCourseProgressDetailAsync(int userId, int courseId)
        {
            var response = new ServiceResponse<CourseProgressDetailDto>();

            try
            {
                var courseProgress = await _courseProgressRepo.GetByUserAndCourseAsync(userId, courseId);

                if (courseProgress == null)
                {
                    response.Success = false;
                    response.Message = $"Course progress not found for user {userId} and course {courseId}";
                    return response;
                }

                // Load course using repository instead of navigation property
                var course = await _courseRepo.GetCourseWithDetails(courseId);

                if (course == null)
                {
                    response.Success = false;
                    response.Message = $"Course with ID {courseId} not found";
                    return response;
                }

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

                            var moduleDto = _mapper.Map<ModuleProgressDetailDto>(module);
                            moduleDto.IsCompleted = moduleCompletion?.IsCompleted ?? false;
                            moduleDto.ProgressPercentage = moduleCompletion?.ProgressPercentage ?? 0;
                            moduleDto.StartedAt = moduleCompletion?.StartedAt;
                            moduleDto.CompletedAt = moduleCompletion?.CompletedAt;

                            moduleDetails.Add(moduleDto);
                        }
                    }
                    else
                    {
                        // No progress yet, return modules with 0% progress
                        foreach (var module in modules)
                        {
                            var moduleDto = _mapper.Map<ModuleProgressDetailDto>(module);
                            moduleDto.IsCompleted = false;
                            moduleDto.ProgressPercentage = 0;
                            moduleDto.StartedAt = null;
                            moduleDto.CompletedAt = null;

                            moduleDetails.Add(moduleDto);
                        }
                    }

                    var lessonDto = _mapper.Map<LessonProgressDetailDto>(lesson);
                    lessonDto.CompletionPercentage = (decimal)(lessonCompletion?.CompletionPercentage ?? 0);
                    lessonDto.CompletedModules = lessonCompletion?.CompletedModules ?? 0;
                    lessonDto.TotalModules = lessonCompletion?.TotalModules ?? modules.Count();
                    lessonDto.VideoProgressPercentage = (decimal)(lessonCompletion?.VideoProgressPercentage ?? 0);
                    lessonDto.IsCompleted = lessonCompletion?.IsCompleted ?? false;
                    lessonDto.StartedAt = lessonCompletion?.StartedAt;
                    lessonDto.CompletedAt = lessonCompletion?.CompletedAt;
                    lessonDto.Modules = moduleDetails;

                    lessonsDetail.Add(lessonDto);
                }

                var courseProgressDetail = _mapper.Map<CourseProgressDetailDto>(course);
                courseProgressDetail.ProgressPercentage = courseProgress.ProgressPercentage;
                courseProgressDetail.CompletedLessons = courseProgress.CompletedLessons;
                courseProgressDetail.TotalLessons = courseProgress.TotalLessons;
                courseProgressDetail.IsCompleted = courseProgress.IsCompleted;
                courseProgressDetail.EnrolledAt = courseProgress.EnrolledAt;
                courseProgressDetail.CompletedAt = courseProgress.CompletedAt;
                courseProgressDetail.LastAccessedAt = null;
                courseProgressDetail.Lessons = lessonsDetail;

                response.Success = true;
                response.Data = courseProgressDetail;
                response.Message = "Course progress detail retrieved successfully";

                _logger.LogInformation("Retrieved course progress detail for user {UserId}, course {CourseId}", userId, courseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course progress detail for user {UserId} and course {CourseId}", userId, courseId);
                response.Success = false;
                response.Message = $"Error retrieving course progress: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> UpdateCourseProgressAsync(int userId, int courseId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var courseProgress = await _courseProgressRepo.GetByUserAndCourseAsync(userId, courseId);

                if (courseProgress == null)
                {
                    response.Success = false;
                    response.Message = $"Course progress not found for user {userId} and course {courseId}";
                    _logger.LogWarning("Course progress not found for user {UserId} and course {CourseId}", userId, courseId);
                    return response;
                }

                courseProgress.UpdateProgress(courseProgress.TotalLessons, courseProgress.CompletedLessons);
                await _courseProgressRepo.UpdateAsync(courseProgress);

                response.Success = true;
                response.Data = true;
                response.Message = "Course progress updated successfully";

                _logger.LogInformation("Updated course progress for user {UserId}, course {CourseId}: {Progress}%",
                    userId, courseId, courseProgress.ProgressPercentage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course progress for user {UserId} and course {CourseId}", userId, courseId);
                response.Success = false;
                response.Message = $"Error updating course progress: {ex.Message}";
            }

            return response;
        }
    }
}