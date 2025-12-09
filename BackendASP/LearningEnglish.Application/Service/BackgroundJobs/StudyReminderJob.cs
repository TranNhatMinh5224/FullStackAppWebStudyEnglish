using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Service.BackgroundJobs
{
    public class StudyReminderJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StudyReminderJob> _logger;

        public StudyReminderJob(IServiceProvider serviceProvider, ILogger<StudyReminderJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Study Reminder Job started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Study Reminder Job");
                }

                // Check every minute
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("Study Reminder Job stopped");
        }

        private async Task CheckAndSendRemindersAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var studyReminderRepository = scope.ServiceProvider.GetRequiredService<IStudyReminderRepository>();
            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
            var flashCardReviewService = scope.ServiceProvider.GetRequiredService<IFlashCardReviewService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            // var pushNotificationService = scope.ServiceProvider.GetRequiredService<IPushNotificationService>();

            var now = DateTime.UtcNow;
            var currentTime = now.ToString("HH:mm");
            var currentDayOfWeek = (DaysOfWeek)(1 << ((int)now.DayOfWeek));

            // Get active reminders for current time and day
            var reminders = await studyReminderRepository.GetActiveRemindersForTimeAsync(currentTime, currentDayOfWeek);

            foreach (var reminder in reminders)
            {
                try
                {
                    // Check if reminder was already sent today
                    if (reminder.LastSentAt?.Date == now.Date)
                    {
                        continue;
                    }

                    // Special handling for FlashcardReview reminders
                    if (reminder.Type == ReminderType.FlashcardReview)
                    {
                        await HandleFlashcardReviewReminder(reminder, flashCardReviewService, notificationService);
                    }

                    // Email sending is disabled in current implementation
                    if (reminder.IsEmailEnabled)
                    {
                        // Feature not yet implemented: Send email notification
                        _logger.LogInformation($"Email reminder scheduled for user {reminder.UserId}: {reminder.Title}");
                    }

                    // Update reminder
                    reminder.LastSentAt = now;
                    reminder.SentCount++;
                    await studyReminderRepository.UpdateAsync(reminder);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error sending reminder {reminder.StudyReminderId} to user {reminder.UserId}");
                }
            }
        }

        private async Task HandleFlashcardReviewReminder(
            StudyReminder reminder,
            IFlashCardReviewService flashCardReviewService,
            INotificationService notificationService)
        {
            try
            {
                // Check if user has due reviews
                var dueCount = await flashCardReviewService.GetDueCountAsync(reminder.UserId);

                if (dueCount > 0)
                {
                    // Create in-app notification
                    var notificationRequest = new CreateNotificationDto
                    {
                        UserId = reminder.UserId,
                        Title = $"📚 {dueCount} từ vựng cần ôn tập",
                        Message = $"Bạn có {dueCount} từ vựng cần ôn tập hôm nay. Hãy dành chút thời gian để củng cố kiến thức!",
                        Type = NotificationType.General,
                        RelatedEntityType = "FlashCardReview",
                        SendEmail = reminder.IsEmailEnabled
                    };

                    await notificationService.CreateNotificationAsync(notificationRequest);

                    _logger.LogInformation($"Flashcard review notification sent to user {reminder.UserId} for {dueCount} due cards");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling flashcard review reminder for user {reminder.UserId}");
            }
        }
    }
}