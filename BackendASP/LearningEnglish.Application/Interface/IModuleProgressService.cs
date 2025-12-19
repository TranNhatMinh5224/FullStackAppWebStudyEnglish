namespace LearningEnglish.Application.Interface;

/// <summary>
/// Service xử lý logic tiến độ module, lesson, course
/// </summary>
public interface IModuleProgressService
{
    /// <summary>
    /// Đánh dấu module hoàn thành (gọi khi user làm xong Quiz/Essay)
    /// - Tự động cập nhật LessonCompletion
    /// - Tự động cập nhật CourseProgress
    /// </summary>
    Task CompleteModuleAsync(int userId, int moduleId);

    /// <summary>
    /// Đánh dấu module bắt đầu (gọi khi user vào module lần đầu)
    /// </summary>
    Task StartModuleAsync(int userId, int moduleId);

    /// <summary>
    /// Start module và tự động complete nếu là FlashCard/Lecture/Video/Reading
    /// Quiz/Essay chỉ start, không auto-complete (phải submit mới complete)
    /// </summary>
    Task StartAndCompleteModuleAsync(int userId, int moduleId);

    /// <summary>
    /// Cập nhật tiến độ video trong lesson
    /// </summary>
    Task UpdateVideoProgressAsync(int userId, int lessonId, int positionSeconds, float videoPercentage);
}
