using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service;

/// <summary>
/// Service đơn giản CHỈ để VocabularyReminderService tạo notification
/// Chỉ có 1 method duy nhất: CreateNotificationAsync
/// </summary>
public class SimpleNotificationService
{
    private readonly INotificationRepository _repository;
    private readonly ILogger<SimpleNotificationService> _logger;

    public SimpleNotificationService(
        INotificationRepository repository,
        ILogger<SimpleNotificationService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Tạo notification đơn giản - CHỈ DÀNH CHO NHẮC HỌC TỪ VỰNG
    /// </summary>
    public async Task<ServiceResponse<bool>> CreateNotificationAsync(int userId, string title, string message)
    {
        var response = new ServiceResponse<bool>();

        try
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(notification);

            response.Data = true;
            response.Success = true;
            response.StatusCode = 200;
            response.Message = "Tạo notification thành công";

            _logger.LogDebug("📤 Tạo notification cho User {UserId}: {Title}", userId, title);
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.StatusCode = 500;
            response.Message = "Lỗi khi tạo notification";

            _logger.LogError(ex, "❌ Lỗi tạo notification cho User {UserId}", userId);
        }

        return response;
    }
}