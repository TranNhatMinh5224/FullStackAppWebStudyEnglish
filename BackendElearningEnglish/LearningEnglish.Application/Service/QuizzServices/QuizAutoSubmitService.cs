using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace LearningEnglish.Application.Service
{
    public class QuizAutoSubmitService : BackgroundService //  QuizAutoSubmitService implement BackgroundService( background là dịch vụ chạy ngầm)
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<QuizAutoSubmitService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Kiểm tra mỗi phút

        public QuizAutoSubmitService(IServiceProvider serviceProvider, ILogger<QuizAutoSubmitService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Quiz Auto-Submit Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndAutoSubmitExpiredAttemptsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in auto-submit check");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task CheckAndAutoSubmitExpiredAttemptsAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var quizAttemptService = scope.ServiceProvider.GetRequiredService<IQuizAttemptService>();
                var result = await quizAttemptService.CheckAndAutoSubmitExpiredAttemptsAsync();

                if (result.Success && result.Data == true)
                {
                    _logger.LogInformation(result.Message);
                }
                else if (!result.Success)
                {
                    _logger.LogError($"Auto-submit failed: {result.Message}");
                }
            }
        }
    }
}