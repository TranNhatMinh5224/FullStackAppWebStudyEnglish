using AutoMapper;
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
    private readonly IMapper _mapper;

    public StreakService(
        IStreakRepository streakRepo,
        ILogger<StreakService> logger,
        IMapper mapper)
    {
        _streakRepo = streakRepo;
        _logger = logger;
        _mapper = mapper;
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

    public async Task<ServiceResponse<StreakUpdateResultDto>> UpdateStreakAsync(int userId)
    {
        var response = new ServiceResponse<StreakUpdateResultDto>();

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

            var today = DateTime.Now.Date; // Use local time for streak calculation
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

}

