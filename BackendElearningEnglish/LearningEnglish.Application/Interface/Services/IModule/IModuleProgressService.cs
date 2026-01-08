using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface.Services.Module;

public interface IModuleProgressService
{
    // Hoàn thành module
    Task<ServiceResponse<object>> CompleteModuleAsync(int userId, int moduleId);

    // Bắt đầu module
    Task<ServiceResponse<object>> StartModuleAsync(int userId, int moduleId);

    // Bắt đầu và hoàn thành module
    Task<ServiceResponse<object>> StartAndCompleteModuleAsync(int userId, int moduleId);

    // Cập nhật tiến độ video
    Task<ServiceResponse<object>> UpdateVideoProgressAsync(int userId, int lessonId, int positionSeconds, float videoPercentage);
}
