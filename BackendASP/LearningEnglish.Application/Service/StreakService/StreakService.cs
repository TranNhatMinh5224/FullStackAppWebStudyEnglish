using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Service;

public class StreakService : IStreakService
{
    private readonly IStreakRepository _streakRepo;
    private readonly ILogger<StreakService> _logger;

    public StreakService(
        IStreakRepository streakRepo,
        ILogger<StreakService> logger)
    {
        _streakRepo = streakRepo;
        _logger = logger;
    }

    public async Task<ServiceResponse<StreakDto>> GetCurrentStreakAsync(int userId)
    {
        try
        {
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

            var streakDto = MapToDto(streak);

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

    public async Task<ServiceResponse<StreakUpdateResultDto>> UpdateStreakAsync(int userId, bool isSuccessful)
    {
        try
        {
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

            var today = DateTime.UtcNow.Date;
            var lastActivity = streak.LastActivityDate?.Date;

            // Chỉ update nếu review thành công
            if (!isSuccessful)
            {
                return new ServiceResponse<StreakUpdateResultDto>
                {
                    Success = true,
                    Data = new StreakUpdateResultDto
                    {
                        Success = false,
                        NewCurrentStreak = streak.CurrentStreak,
                        NewLongestStreak = streak.LongestStreak,
                        IsNewRecord = false,
                        Message = "Review không thành công, streak không được cập nhật"
                    },
                    Message = "Review không thành công"
                };
            }

            // Nếu đã update hôm nay rồi, không làm gì
            if (lastActivity == today)
            {
                return new ServiceResponse<StreakUpdateResultDto>
                {
                    Success = true,
                    Data = new StreakUpdateResultDto
                    {
                        Success = true,
                        NewCurrentStreak = streak.CurrentStreak,
                        NewLongestStreak = streak.LongestStreak,
                        IsNewRecord = false,
                        Message = "Đã cập nhật streak hôm nay rồi"
                    },
                    Message = "Streak đã được cập nhật hôm nay"
                };
            }

            bool isNewRecord = false;

            // Logic cập nhật streak
            if (lastActivity == null)
            {
                // Lần đầu tiên
                streak.CurrentStreak = 1;
                streak.CurrentStreakStartDate = today;
                streak.TotalActiveDays = 1;
            }
            else if (lastActivity == today.AddDays(-1))
            {
                // Tiếp tục streak (học liên tục)
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

            var result = new StreakUpdateResultDto
            {
                Success = true,
                NewCurrentStreak = streak.CurrentStreak,
                NewLongestStreak = streak.LongestStreak,
                IsNewRecord = isNewRecord,
                Message = isNewRecord 
                    ? $"🎉 Kỷ lục mới! Streak hiện tại: {streak.CurrentStreak} ngày" 
                    : $"Streak hiện tại: {streak.CurrentStreak} ngày"
            };

            return new ServiceResponse<StreakUpdateResultDto>
            {
                Success = true,
                Data = result,
                Message = "Cập nhật streak thành công"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating streak for user {UserId}", userId);
            return new ServiceResponse<StreakUpdateResultDto>
            {
                Success = false,
                Message = $"Không thể cập nhật streak: {ex.Message}"
            };
        }
    }

    private StreakDto MapToDto(Streak streak)
    {
        var today = DateTime.UtcNow.Date;
        var lastActivity = streak.LastActivityDate?.Date;

        return new StreakDto
        {
            UserId = streak.UserId,
            CurrentStreak = streak.CurrentStreak,
            LastActivityDate = streak.LastActivityDate,
            IsActiveToday = lastActivity == today
        };
    }
}

