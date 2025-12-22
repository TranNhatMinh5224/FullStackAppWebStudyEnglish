using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service;

/// <summary>
/// Service xử lý tự động cập nhật tiến độ Module → Lesson → Course
/// </summary>
public class ModuleProgressService : IModuleProgressService
{
    private readonly IModuleCompletionRepository _moduleCompletionRepo;
    private readonly ILessonCompletionRepository _lessonCompletionRepo;
    private readonly ICourseProgressRepository _courseProgressRepo;
    private readonly IModuleRepository _moduleRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger<ModuleProgressService> _logger;

    public ModuleProgressService(
        IModuleCompletionRepository moduleCompletionRepo,
        ILessonCompletionRepository lessonCompletionRepo,
        ICourseProgressRepository courseProgressRepo,
        IModuleRepository moduleRepository,
        ILessonRepository lessonRepository,
        INotificationRepository notificationRepository,
        ICourseRepository courseRepository,
        ILogger<ModuleProgressService> logger)
    {
        _moduleCompletionRepo = moduleCompletionRepo;
        _lessonCompletionRepo = lessonCompletionRepo;
        _courseProgressRepo = courseProgressRepo;
        _moduleRepository = moduleRepository;
        _lessonRepository = lessonRepository;
        _notificationRepository = notificationRepository;
        _courseRepository = courseRepository;
        _logger = logger;
    }

    /// <summary>
    /// Đánh dấu module hoàn thành và tự động cập nhật Lesson + Course progress
    /// </summary>
    public async Task<ServiceResponse<object>> CompleteModuleAsync(int userId, int moduleId)
    {
        try
        {
            // 1. Lấy thông tin module để biết thuộc Lesson nào
            var module = await _moduleRepository.GetByIdAsync(moduleId);
            if (module == null)
            {
                _logger.LogWarning("Module {ModuleId} không tồn tại", moduleId);
                return new ServiceResponse<object>
                {
                    Success = false,
                    StatusCode = 404,
                    Message = "Module không tồn tại"
                };
            }

            // 2. Đánh dấu module hoàn thành
            var moduleCompletion = await _moduleCompletionRepo.GetByUserAndModuleAsync(userId, moduleId);
            if (moduleCompletion == null)
            {
                // Tạo mới nếu chưa tồn tại
                moduleCompletion = new ModuleCompletion
                {
                    UserId = userId,
                    ModuleId = moduleId
                };
                moduleCompletion.MarkAsCompleted();
                await _moduleCompletionRepo.AddAsync(moduleCompletion);
                _logger.LogInformation("User {UserId} hoàn thành module {ModuleId}", userId, moduleId);
            }
            else if (!moduleCompletion.IsCompleted)
            {
                // Cập nhật nếu đã tồn tại nhưng chưa completed
                moduleCompletion.MarkAsCompleted();
                await _moduleCompletionRepo.UpdateAsync(moduleCompletion);
                _logger.LogInformation("User {UserId} cập nhật hoàn thành module {ModuleId}", userId, moduleId);
            }

            // 3. Cập nhật LessonCompletion (tính lại % hoàn thành)
            await UpdateLessonProgressAsync(userId, module.LessonId);

            // 4. Cập nhật CourseProgress (tính lại % hoàn thành)
            var lesson = await _lessonRepository.GetLessonById(module.LessonId);
            if (lesson != null)
            {
                await UpdateCourseProgressAsync(userId, lesson.CourseId);
            }

            _logger.LogInformation("✅ User {UserId} hoàn thành module {ModuleId} - CẦN GỬI NOTIFICATION", 
                userId, moduleId);
                
            return new ServiceResponse<object>
            {
                Success = true,
                StatusCode = 200,
                Message = "Module completed successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi hoàn thành module {ModuleId} cho user {UserId}", moduleId, userId);
            return new ServiceResponse<object>
            {
                Success = false,
                StatusCode = 500,
                Message = "Lỗi khi hoàn thành module"
            };
        }
    }

    /// <summary>
    /// Đánh dấu module bắt đầu (khi user vào module lần đầu)
    /// </summary>
    public async Task<ServiceResponse<object>> StartModuleAsync(int userId, int moduleId)
    {
        try
        {
            var moduleCompletion = await _moduleCompletionRepo.GetByUserAndModuleAsync(userId, moduleId);
            if (moduleCompletion == null)
            {
                // Tạo mới với trạng thái started
                moduleCompletion = new ModuleCompletion
                {
                    UserId = userId,
                    ModuleId = moduleId
                };
                moduleCompletion.MarkAsStarted();
                await _moduleCompletionRepo.AddAsync(moduleCompletion);
                _logger.LogInformation("User {UserId} bắt đầu module {ModuleId}", userId, moduleId);
            }
            
            return new ServiceResponse<object>
            {
                Success = true,
                StatusCode = 200,
                Message = "Module started successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi bắt đầu module {ModuleId} cho user {UserId}", moduleId, userId);
            return new ServiceResponse<object>
            {
                Success = false,
                StatusCode = 500,
                Message = "Lỗi khi bắt đầu module"
            };
        }
    }

    /// <summary>
    /// Cập nhật tiến độ video trong lesson
    /// </summary>
    public async Task<ServiceResponse<object>> UpdateVideoProgressAsync(int userId, int lessonId, int positionSeconds, float videoPercentage)
    {
        try
        {
            var lessonCompletion = await _lessonCompletionRepo.GetByUserAndLessonAsync(userId, lessonId);
            if (lessonCompletion == null)
            {
                // Tạo mới
                lessonCompletion = new LessonCompletion
                {
                    UserId = userId,
                    LessonId = lessonId
                };
                lessonCompletion.MarkVideoProgress(positionSeconds, videoPercentage);
                await _lessonCompletionRepo.AddAsync(lessonCompletion);
            }
            else
            {
                // Cập nhật
                lessonCompletion.MarkVideoProgress(positionSeconds, videoPercentage);
                await _lessonCompletionRepo.UpdateAsync(lessonCompletion);
            }
            
            return new ServiceResponse<object>
            {
                Success = true,
                StatusCode = 200,
                Message = "Video progress updated successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật video progress lesson {LessonId} cho user {UserId}", lessonId, userId);
            return new ServiceResponse<object>
            {
                Success = false,
                StatusCode = 500,
                Message = "Lỗi khi cập nhật tiến độ video"
            };
        }
    }

    /// <summary>
    /// Start module và tự động complete nếu là FlashCard/Lecture/Video/Reading
    /// Quiz/Essay chỉ start, không auto-complete
    /// </summary>
    public async Task<ServiceResponse<object>> StartAndCompleteModuleAsync(int userId, int moduleId)
    {
        try
        {
            // Lấy thông tin module
            var module = await _moduleRepository.GetByIdAsync(moduleId);
            if (module == null)
            {
                _logger.LogWarning("Module {ModuleId} không tồn tại", moduleId);
                return new ServiceResponse<object>
                {
                    Success = false,
                    StatusCode = 404,
                    Message = "Module không tồn tại"
                };
            }

            // Start module
            await StartModuleAsync(userId, moduleId);

            // Auto-complete cho các module types: FlashCard, Lecture, Video, Reading
            // Quiz và Assignment phải submit mới complete
            if (module.ContentType == ModuleType.FlashCard || 
                module.ContentType == ModuleType.Lecture || 
                module.ContentType == ModuleType.Video || 
                module.ContentType == ModuleType.Reading)
            {
                await CompleteModuleAsync(userId, moduleId);
                _logger.LogInformation(
                    "✅ User {UserId} auto-completed {ContentType} module {ModuleId} when entering",
                    userId, module.ContentType, moduleId);
            }
            else
            {
                _logger.LogInformation(
                    "📝 User {UserId} started {ContentType} module {ModuleId} - requires submission to complete",
                    userId, module.ContentType, moduleId);
            }
            
            return new ServiceResponse<object>
            {
                Success = true,
                StatusCode = 200,
                Message = "Module started successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi start module {ModuleId} cho user {UserId}", moduleId, userId);
            return new ServiceResponse<object>
            {
                Success = false,
                StatusCode = 500,
                Message = "Lỗi khi bắt đầu module"
            };
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Cập nhật LessonCompletion dựa trên số lượng modules hoàn thành
    /// </summary>
    private async Task UpdateLessonProgressAsync(int userId, int lessonId)
    {
        try
        {
            // Đếm tổng số modules trong lesson
            var modules = await _moduleRepository.GetByLessonIdAsync(lessonId);
            var totalModules = modules.Count;

            // Đếm số modules đã hoàn thành
            var moduleIds = modules.Select(m => m.ModuleId).ToList();
            var completedModules = await _moduleCompletionRepo.GetByUserAndModuleIdsAsync(userId, moduleIds);
            var completedCount = completedModules.Count(mc => mc.IsCompleted);

            // Cập nhật LessonCompletion
            var lessonCompletion = await _lessonCompletionRepo.GetByUserAndLessonAsync(userId, lessonId);
            if (lessonCompletion == null)
            {
                // Tạo mới
                lessonCompletion = new LessonCompletion
                {
                    UserId = userId,
                    LessonId = lessonId
                };
                lessonCompletion.UpdateModuleProgress(totalModules, completedCount);
                await _lessonCompletionRepo.AddAsync(lessonCompletion);
            }
            else
            {
                // Cập nhật
                lessonCompletion.UpdateModuleProgress(totalModules, completedCount);
                await _lessonCompletionRepo.UpdateAsync(lessonCompletion);
            }

            _logger.LogInformation(
                "Lesson {LessonId} progress updated: {Completed}/{Total} modules",
                lessonId, completedCount, totalModules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật lesson progress {LessonId} cho user {UserId}", lessonId, userId);
            // Don't throw - lesson progress update is non-critical, module is already completed
        }
    }

    /// <summary>
    /// Cập nhật CourseProgress dựa trên số lượng lessons hoàn thành
    /// </summary>
    private async Task UpdateCourseProgressAsync(int userId, int courseId)
    {
        try
        {
            // Đếm tổng số lessons trong course
            var lessons = await _lessonRepository.GetListLessonByCourseId(courseId);
            var totalLessons = lessons.Count;

            // Đếm số lessons đã hoàn thành (>= 90% modules)
            var lessonIds = lessons.Select(l => l.LessonId).ToList();
            var lessonCompletions = await _lessonCompletionRepo.GetByUserAndLessonIdsAsync(userId, lessonIds);
            var completedCount = lessonCompletions.Count(lc => lc.IsCompleted);

            // Cập nhật CourseProgress
            var courseProgress = await _courseProgressRepo.GetByUserAndCourseAsync(userId, courseId);
            if (courseProgress == null)
            {
                // Tạo mới
                courseProgress = new CourseProgress
                {
                    UserId = userId,
                    CourseId = courseId
                };
                courseProgress.UpdateProgress(totalLessons, completedCount);
                await _courseProgressRepo.AddAsync(courseProgress);
            }
            else
            {
                // Cập nhật
                courseProgress.UpdateProgress(totalLessons, completedCount);
                await _courseProgressRepo.UpdateAsync(courseProgress);
            }

            _logger.LogInformation(
                "Course {CourseId} progress updated: {Completed}/{Total} lessons",
                courseId, completedCount, totalLessons);

            // Gửi notification nếu hoàn thành 100% khóa học
            if (courseProgress.IsCompleted)
            {
                await CreateCourseCompletionNotificationAsync(userId, courseId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật course progress {CourseId} cho user {UserId}", courseId, userId);
            // Don't throw - course progress update is non-critical, module is already completed
        }
    }

    private async Task CreateCourseCompletionNotificationAsync(int userId, int courseId)
    {
        try
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course != null)
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = "🎓 Chúc mừng! Bạn đã hoàn thành khóa học",
                    Message = $"Bạn đã hoàn thành 100% khóa học '{course.Title}'. Thật tuyệt vời!",
                    Type = NotificationType.CourseCompletion,
                    RelatedEntityType = "Course",
                    RelatedEntityId = courseId,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationRepository.AddAsync(notification);
                _logger.LogInformation("Created course completion notification for user {UserId}, course {CourseId}", userId, courseId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create course completion notification");
        }
    }

    #endregion
}
