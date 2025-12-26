using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Service;

public class StreakService : IStreakService
{
    private readonly IStreakRepository _streakRepo;
    private readonly INotificationRepository _notificationRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<StreakService> _logger;
    private readonly IMapper _mapper;

    public StreakService(
        IStreakRepository streakRepo,
        INotificationRepository notificationRepository,
        IEmailService emailService,
        ILogger<StreakService> logger,
        IMapper mapper)
    {
        _streakRepo = streakRepo;
        _notificationRepository = notificationRepository;
        _emailService = emailService;
        _logger = logger;
        _mapper = mapper;
    }

    // Lấy streak hiện tại của user
    // RLS đã filter: User chỉ xem streak của chính mình, Admin xem tất cả (có permission)
    public async Task<ServiceResponse<StreakDto>> GetCurrentStreakAsync(int userId)
    {
        try
        {
            // RLS đã filter theo userId
            var streak = await _streakRepo.GetByUserIdAsync(userId);

            if (streak == null)
            {
                // Tạo streak mới nếu chưa có
                streak = new Streak
                {
                    UserId = userId,
                    CurrentStreak = 0,
                    LongestStreak = 0,
                    TotalActiveDays = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                streak = await _streakRepo.CreateAsync(streak);
            }

            var streakDto = _mapper.Map<StreakDto>(streak);

            return new ServiceResponse<StreakDto>
            {
                Success = true,
                Data = streakDto,
                Message = "Lấy streak thành công"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current streak for user {UserId}", userId);
            return new ServiceResponse<StreakDto>
            {
                Success = false,
                Message = $"Không thể lấy streak hiện tại: {ex.Message}"
            };
        }
    }

    // Cập nhật streak khi user online
    // RLS đã filter: User chỉ update streak của chính mình
    public async Task<ServiceResponse<StreakUpdateResultDto>> UpdateStreakAsync(int userId)
    {
        var response = new ServiceResponse<StreakUpdateResultDto>();

        try
        {
            // RLS đã filter theo userId
            var streak = await _streakRepo.GetByUserIdAsync(userId);

            if (streak == null)
            {
                // Tạo mới nếu chưa có
                streak = new Streak
                {
                    UserId = userId,
                    CurrentStreak = 0,
                    LongestStreak = 0,
                    TotalActiveDays = 0,
                    CreatedAt = DateTime.UtcNow
                };
            }

            var today = DateTime.UtcNow.Date; // Use UTC time for PostgreSQL compatibility
            var lastActivity = streak.LastActivityDate?.Date;

            // Nếu đã update hôm nay rồi, không làm gì
            if (lastActivity == today)
            {
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Streak đã được cập nhật hôm nay";
                response.Data = new StreakUpdateResultDto
                {
                    Success = true,
                    NewCurrentStreak = streak.CurrentStreak,
                    NewLongestStreak = streak.LongestStreak,
                    IsNewRecord = false,
                    Message = "Đã cập nhật streak hôm nay rồi"
                };
                return response;
            }

            bool isNewRecord = false;

            // Logic cập nhật streak - chỉ cần user online là được
            if (lastActivity == null)
            {
                // Lần đầu tiên
                streak.CurrentStreak = 1;
                streak.CurrentStreakStartDate = today;
                streak.TotalActiveDays = 1;
            }
            else if (lastActivity == today.AddDays(-1))
            {
                // Tiếp tục streak (online liên tục)
                streak.CurrentStreak++;
                streak.TotalActiveDays++;
            }
            else if (lastActivity < today.AddDays(-1))
            {
                // Bị đứt streak (bỏ quá 1 ngày)
                streak.CurrentStreak = 1;
                streak.CurrentStreakStartDate = today;
                streak.TotalActiveDays++;
            }

            // Cập nhật longest streak nếu phá kỷ lục
            if (streak.CurrentStreak > streak.LongestStreak)
            {
                streak.LongestStreak = streak.CurrentStreak;
                isNewRecord = true;
            }

            streak.LastActivityDate = today;
            streak.UpdatedAt = DateTime.UtcNow;

            if (streak.StreakId == 0)
            {
                await _streakRepo.CreateAsync(streak);
            }
            else
            {
                await _streakRepo.UpdateAsync(streak);
            }

            response.Success = true;
            response.StatusCode = 200;
            response.Message = "Cập nhật streak thành công";
            response.Data = new StreakUpdateResultDto
            {
                Success = true,
                NewCurrentStreak = streak.CurrentStreak,
                NewLongestStreak = streak.LongestStreak,
                IsNewRecord = isNewRecord,
                Message = isNewRecord
                    ? $"🎉 Kỷ lục mới! Streak hiện tại: {streak.CurrentStreak} ngày"
                    : $"Streak hiện tại: {streak.CurrentStreak} ngày"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating streak for user {UserId}", userId);
            response.Success = false;
            response.StatusCode = 500;
            response.Message = $"Không thể cập nhật streak: {ex.Message}";
        }

        return response;
    }

    // Gửi reminder cho users sắp đứt streak (Admin/Cron job)
    // RLS đã filter: Chỉ Admin có permission Admin.User.Manage mới xem được tất cả streaks
    public async Task<ServiceResponse<object>> SendStreakRemindersAsync()
    {
        try
        {
            // RLS đã filter: Chỉ Admin có permission mới xem được
            // Lấy users có streak >= 3 ngày và LastActivityDate = yesterday (sắp đứt streak)
            var usersAtRisk = await _streakRepo.GetUsersAtRiskOfLosingStreakAsync(minStreak: 3);
            
            if (usersAtRisk.Count == 0)
            {
                _logger.LogInformation("No users at risk of losing streak today");
                return new ServiceResponse<object>
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "No users need streak reminders today",
                    Data = new { Count = 0 }
                };
            }

            int successCount = 0;
            int failedCount = 0;

            foreach (var streak in usersAtRisk)
            {
                try
                {
                    var user = streak.User;
                    if (user == null || string.IsNullOrEmpty(user.Email))
                    {
                        _logger.LogWarning("User not found or email missing for streak {StreakId}", streak.StreakId);
                        failedCount++;
                        continue;
                    }

                    // 1. Tạo notification trong hệ thống
                    var notification = new Notification
                    {
                        UserId = user.UserId,
                        Title = $"🔥 Streak {streak.CurrentStreak} ngày của bạn sắp đứt!",
                        Message = $"Bạn chưa học hôm nay! Hãy dành vài phút để giữ streak {streak.CurrentStreak} ngày và tiếp tục tiến bộ.",
                        Type = NotificationType.StreakReminder,
                        RelatedEntityType = "Streak",
                        RelatedEntityId = streak.StreakId,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _notificationRepository.AddAsync(notification);

                    // 2. Gửi email reminder
                    await _emailService.SendStreakReminderEmailAsync(
                        user.Email,
                        user.FullName,
                        streak.CurrentStreak,
                        streak.LongestStreak
                    );

                    successCount++;
                    _logger.LogInformation(
                        "Sent streak reminder to user {UserId} ({Email}) - Streak: {CurrentStreak} days",
                        user.UserId, user.Email, streak.CurrentStreak);
                }
                catch (Exception ex)
                {
                    failedCount++;
                    _logger.LogError(ex, "Failed to send streak reminder for streak {StreakId}", streak.StreakId);
                }
            }

            return new ServiceResponse<object>
            {
                Success = true,
                StatusCode = 200,
                Message = $"Sent {successCount} streak reminders successfully",
                Data = new
                {
                    TotalUsers = usersAtRisk.Count,
                    Success = successCount,
                    Failed = failedCount
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending streak reminders");
            return new ServiceResponse<object>
            {
                Success = false,
                StatusCode = 500,
                Message = $"Không thể gửi streak reminders: {ex.Message}"
            };
        }
    }

}
