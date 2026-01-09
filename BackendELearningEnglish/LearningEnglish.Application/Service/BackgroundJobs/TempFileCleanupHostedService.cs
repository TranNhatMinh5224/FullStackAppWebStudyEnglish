using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service.BackgroundJobs
{
    // Background service chạy cleanup temp files định kỳ mỗi 6 giờ
    public class TempFileCleanupHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TempFileCleanupHostedService> _logger;
        private readonly TimeSpan _interval;

        public TempFileCleanupHostedService(
            IServiceProvider serviceProvider,
            ILogger<TempFileCleanupHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            // Chạy mỗi 6 giờ
            _interval = TimeSpan.FromHours(6);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Temp File Cleanup Hosted Service is starting");

            // Đợi 1 phút sau khi app start mới chạy lần đầu
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Running temp file cleanup at {Time}", DateTime.UtcNow);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var cleanupJob = scope.ServiceProvider.GetRequiredService<TempFileCleanupJob>();
                        await cleanupJob.CleanupOldTempFilesAsync();
                    }

                    _logger.LogInformation("Temp file cleanup completed. Next run in {Hours} hours", _interval.TotalHours);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during temp file cleanup");
                }

                // Đợi đến lần chạy tiếp theo
                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Temp File Cleanup Hosted Service is stopping");
        }
    }
}
