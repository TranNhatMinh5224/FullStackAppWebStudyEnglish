using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service.BackgroundJobs
{
    /// <summary>
    /// Background service để cleanup các payment Pending đã hết hạn (ExpiredAt)
    /// Chạy định kỳ mỗi 1 giờ để update Status = Expired
    /// </summary>
    public class PaymentCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<PaymentCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1); // Chạy mỗi giờ

        public PaymentCleanupService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<PaymentCleanupService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("💳 Payment Cleanup Service started - Running every {Interval} hour(s)",
                _cleanupInterval.TotalHours);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredPaymentsAsync();

                    // Chờ 1 giờ trước khi chạy lần tiếp theo
                    await Task.Delay(_cleanupInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Service đang dừng, không log error
                    _logger.LogInformation("⏹️ Payment Cleanup Service stopping...");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error in Payment Cleanup Service");

                    // Chờ 5 phút trước khi retry nếu có lỗi
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("✅ Payment Cleanup Service stopped");
        }

        private async Task CleanupExpiredPaymentsAsync()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var paymentRepository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

            try
            {
                _logger.LogInformation("🔍 Starting expired payment cleanup at {Time}", DateTime.UtcNow);

                // Grace period 1 giờ: Chỉ cleanup payments đã hết hạn > 1 giờ
                // Tránh cleanup payment vừa mới hết hạn (user có thể đang thanh toán)
                var cutoffTime = DateTime.UtcNow.AddHours(-1);

                // Batch processing để tránh load quá nhiều vào memory
                const int BATCH_SIZE = 500;
                int skip = 0;
                int totalUpdated = 0;
                bool hasMore = true;

                while (hasMore)
                {
                    var expiredPayments = await paymentRepository.GetExpiredPendingPaymentsAsync(cutoffTime);
                    var batch = expiredPayments.Skip(skip).Take(BATCH_SIZE).ToList();

                    if (!batch.Any())
                    {
                        hasMore = false;
                        break;
                    }

                    _logger.LogInformation("🧹 Processing batch: {Count} expired payments (offset: {Skip})", 
                        batch.Count, skip);

                    try
                    {
                        // Update batch
                        foreach (var payment in batch)
                        {
                            payment.Status = PaymentStatus.Expired;
                            payment.UpdatedAt = DateTime.UtcNow;
                            payment.ErrorMessage = "Payment link expired - auto cleanup";

                            await paymentRepository.UpdatePaymentStatusAsync(payment);

                            _logger.LogDebug("Expired Payment {PaymentId} - OrderCode: {OrderCode}, ExpiredAt: {ExpiredAt}",
                                payment.PaymentId, payment.OrderCode, payment.ExpiredAt);
                        }

                        await paymentRepository.SaveChangesAsync();
                        totalUpdated += batch.Count;
                        
                        _logger.LogInformation("✅ Batch completed: {Count} payments updated", batch.Count);
                    }
                    catch (Exception batchEx)
                    {
                        _logger.LogError(batchEx, "❌ Failed to update batch at offset {Skip}. Continuing with next batch.", skip);
                        // Continue với batch tiếp theo thay vì dừng hoàn toàn
                    }

                    skip += BATCH_SIZE;

                    // Nếu batch nhỏ hơn BATCH_SIZE → đây là batch cuối
                    if (batch.Count < BATCH_SIZE)
                    {
                        hasMore = false;
                    }

                    // Small delay để không overwhelm database
                    if (hasMore)
                    {
                        await Task.Delay(100);
                    }
                }

                if (totalUpdated > 0)
                {
                    _logger.LogInformation("✅ Successfully cleaned up {Count} expired payments at {Time}",
                        totalUpdated, DateTime.UtcNow);
                }
                else
                {
                    _logger.LogInformation("✅ No expired payments found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to cleanup expired payments");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("💳 Payment Cleanup Service shutdown initiated");
            await base.StopAsync(stoppingToken);
        }
    }
}
