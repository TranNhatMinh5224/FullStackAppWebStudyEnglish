using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public async Task<ServiceResponse<int>> GetLongestStreakAsync(int userId)
    {
        try
        {
            var streak = await _streakRepo.GetByUserIdAsync(userId);

            if (streak == null)
            {
                return new ServiceResponse<int>
                {
                    Success = true,
                    Data = 0,
                    Message = "Chưa có dữ liệu streak"
                };
            }

            return new ServiceResponse<int>
            {
                Success = true,
                Data = streak.LongestStreak,
                Message = "Lấy longest streak thành công"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting longest streak for user {UserId}", userId);
            return new ServiceResponse<int>
            {
                Success = false,
                Message = $"Không thể lấy streak dài nhất: {ex.Message}"
            };
        }
    }

    public async Task<ServiceResponse<List<StreakHistoryDto>>> GetStreakHistoryAsync(int userId, int days = 30)
    {
        try
        {
            var streak = await _streakRepo.GetByUserIdAsync(userId);

            if (streak == null || streak.LastActivityDate == null)
            {
                return new ServiceResponse<List<StreakHistoryDto>>
                {
                    Success = true,
                    Data = new List<StreakHistoryDto>(),
                    Message = "Chưa có lịch sử streak"
                };
            }

            // TODO: Implement proper history tracking
            // For now, generate mock history based on current streak
            var history = new List<StreakHistoryDto>();
            var today = DateTime.UtcNow.Date;
            var startDate = streak.CurrentStreakStartDate ?? today.AddDays(-days);

            for (int i = 0; i < days; i++)
            {
                var date = today.AddDays(-i);
                
                bool wasActive = false;
                int streakOnThatDay = 0;

                if (date >= startDate && date <= streak.LastActivityDate?.Date)
                {
                    wasActive = true;
                    streakOnThatDay = (int)(streak.LastActivityDate.Value.Date - date).TotalDays + 1;
                }

                history.Add(new StreakHistoryDto
                {
                    Date = date,
                    WasActive = wasActive,
                    StreakOnThatDay = streakOnThatDay
                });
            }

            return new ServiceResponse<List<StreakHistoryDto>>
            {
                Success = true,
                Data = history.OrderBy(h => h.Date).ToList(),
                Message = "Lấy lịch sử streak thành công"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting streak history for user {UserId}", userId);
            return new ServiceResponse<List<StreakHistoryDto>>
            {
                Success = false,
                Message = $"Không thể lấy lịch sử streak: {ex.Message}"
            };
        }
    }

    public async Task<ServiceResponse<StreakDto>> ResetStreakAsync(int userId)
    {
        try
        {
            var streak = await _streakRepo.GetByUserIdAsync(userId);

            if (streak == null)
            {
                return new ServiceResponse<StreakDto>
                {
                    Success = false,
                    Message = "Không tìm thấy streak để reset"
                };
            }

            // Reset streak về 0
            streak.CurrentStreak = 0;
            streak.CurrentStreakStartDate = null;
            streak.LastActivityDate = null;
            streak.UpdatedAt = DateTime.UtcNow;

            await _streakRepo.UpdateAsync(streak);

            return new ServiceResponse<StreakDto>
            {
                Success = true,
                Data = MapToDto(streak),
                Message = "Reset streak thành công"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting streak for user {UserId}", userId);
            return new ServiceResponse<StreakDto>
            {
                Success = false,
                Message = $"Không thể reset streak: {ex.Message}"
            };
        }
    }

    private StreakDto MapToDto(Streak streak)
    {
        var today = DateTime.UtcNow.Date;
        var lastActivity = streak.LastActivityDate?.Date;

        string status;
        if (lastActivity == null)
        {
            status = "New";
        }
        else if (lastActivity == today)
        {
            status = "Active";
        }
        else if (lastActivity == today.AddDays(-1))
        {
            status = "Active";
        }
        else
        {
            status = "Broken";
        }

        return new StreakDto
        {
            UserId = streak.UserId,
            CurrentStreak = streak.CurrentStreak,
            LongestStreak = streak.LongestStreak,
            TotalActiveDays = streak.TotalActiveDays,
            LastActivityDate = streak.LastActivityDate,
            CurrentStreakStartDate = streak.CurrentStreakStartDate,
            IsActiveToday = lastActivity == today,
            StreakStatus = status
        };
    }
}

