using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service;

// Service tạo notification cho tất cả các loại thông báo hệ thống
// Lưu ý: Service này chỉ được sử dụng bởi system/background services hoặc các service khác trong nội bộ
// Không được expose qua controller để đảm bảo bảo mật
public class SimpleNotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SimpleNotificationService> _logger;

    public SimpleNotificationService(
        INotificationRepository repository,
        IUserRepository userRepository,
        ILogger<SimpleNotificationService> logger)
    {
        _repository = repository;
        _userRepository = userRepository;
        _logger = logger;
    }

    // Tạo notification - Dùng cho tất cả các loại thông báo
    // Lưu ý: Service này không check ownership/enrollment vì được sử dụng bởi system services
    // Các service gọi method này phải tự chịu trách nhiệm về việc check ownership/enrollment trước khi gọi
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
            // Validation: Kiểm tra user tồn tại
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                response.Success = false;
                response.StatusCode = 404;
                response.Message = $"User với ID {userId} không tồn tại";
                _logger.LogWarning("Attempted to create notification for non-existent user {UserId}", userId);
                return response;
            }

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

            _logger.LogDebug("Tạo notification cho User {UserId}: {Title}", userId, title);
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.StatusCode = 500;
            response.Message = "Lỗi khi tạo notification";

            _logger.LogError(ex, "Lỗi tạo notification cho User {UserId}", userId);
        }

        return response;
    }
}