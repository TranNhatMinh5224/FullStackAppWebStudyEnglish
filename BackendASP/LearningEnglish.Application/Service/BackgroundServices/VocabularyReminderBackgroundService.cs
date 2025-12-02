using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace LearningEnglish.Application.Service.BackgroundServices
{
    /// <summary>
    /// Background service to send daily vocabulary review reminders
    /// Runs every day at 9:00 AM to notify users who have flashcards due for review
    /// </summary>
    public class VocabularyReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VocabularyReminderBackgroundService> _logger;
        private readonly TimeSpan _reminderTime = new TimeSpan(9, 0, 0); // 9:00 AM

        public VocabularyReminderBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<VocabularyReminderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("VocabularyReminderBackgroundService đã khởi động");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var nextRunTime = CalculateNextRunTime(now);
                    var delay = nextRunTime - now;

                    _logger.LogInformation("Nhắc nhở học từ vựng tiếp theo lúc: {NextRunTime}", nextRunTime);

                    await Task.Delay(delay, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await SendVocabularyRemindersAsync();
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("VocabularyReminderBackgroundService đang dừng...");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi trong VocabularyReminderBackgroundService");
                    
                    // Wait 1 hour before retry in case of error
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }

            _logger.LogInformation("VocabularyReminderBackgroundService đã dừng");
        }

        private async Task SendVocabularyRemindersAsync()
        {
            _logger.LogInformation("Bắt đầu gửi nhắc nhở học từ vựng...");

            using var scope = _serviceProvider.CreateScope();
            var reviewRepository = scope.ServiceProvider.GetRequiredService<IFlashCardReviewRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            try
            {
                var currentDate = DateTime.UtcNow.Date;
                
                // Get all users - filter for students with Role "Student"
                var allUsers = await userRepository.GetAllUsersAsync();
                var students = allUsers.Where(u => u.Roles.Any(r => r.Name == "Student")).ToList();

                int remindersSent = 0;
                int usersWithDueCards = 0;

                foreach (var student in students)
                {
                    try
                    {
                        // Check if student has flashcards due today
                        var dueCount = await reviewRepository.GetDueCountAsync(student.UserId, currentDate);

                        if (dueCount > 0)
                        {
                            usersWithDueCards++;

                            // Create notification
                            var notificationDto = new CreateNotificationDto
                            {
                                UserId = student.UserId,
                                Title = "🔔 Nhắc nhở ôn tập từ vựng",
                                Message = $"Bạn có {dueCount} từ vựng cần ôn tập hôm nay! Hãy dành thời gian ôn lại để ghi nhớ tốt hơn nhé! 📚",
                                Type = NotificationType.LessonReminder,
                                RelatedEntityType = "FlashCardReview",
                                RelatedEntityId = null,
                                SendEmail = false // Set to true if you want email notifications
                            };

                            var result = await notificationService.CreateNotificationAsync(notificationDto);

                            if (result.Success)
                            {
                                remindersSent++;
                                _logger.LogInformation("Đã gửi nhắc nhở cho user {UserId} ({Email}) - {DueCount} từ cần ôn", 
                                    student.UserId, student.Email, dueCount);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi khi gửi nhắc nhở cho user {UserId}", student.UserId);
                    }
                }

                _logger.LogInformation("Hoàn thành gửi nhắc nhở: {RemindersSent}/{UsersWithDueCards} thành công", 
                    remindersSent, usersWithDueCards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi vocabulary reminders");
            }
        }

        private DateTime CalculateNextRunTime(DateTime currentTime)
        {
            var scheduledTime = currentTime.Date + _reminderTime;

            // If scheduled time has already passed today, schedule for tomorrow
            if (currentTime >= scheduledTime)
            {
                scheduledTime = scheduledTime.AddDays(1);
            }

            return scheduledTime;
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("VocabularyReminderBackgroundService đang dừng lại...");
            await base.StopAsync(stoppingToken);
        }
    }
}
