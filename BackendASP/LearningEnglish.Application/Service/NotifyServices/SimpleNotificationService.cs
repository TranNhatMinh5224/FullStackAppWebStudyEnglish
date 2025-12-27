using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service;

// Service tạo notification cho tất cả các loại thông báo hệ thống
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

    // Tạo notification - Dùng cho tất cả các loại thông báo
    public async Task<ServiceResponse<bool>> CreateNotificationAsync(
        int userId, 
        string title, 
        string message, 
        NotificationType type,
        string? relatedEntityType = null,
        int? relatedEntityId = null)
    {
        var response = new ServiceResponse<bool>();

        try
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                RelatedEntityType = relatedEntityType,
                RelatedEntityId = relatedEntityId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(notification);

            response.Data = true;
            response.Success = true;
            response.StatusCode = 200;
            response.Message = "Tạo notification thành công";

            _logger.LogDebug(" Tạo notification cho User {UserId}: {Title}", userId, title);
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.StatusCode = 500;
            response.Message = "Lỗi khi tạo notification";

            _logger.LogError(ex, " Lỗi tạo notification cho User {UserId}", userId);
        }

        return response;
    }
}