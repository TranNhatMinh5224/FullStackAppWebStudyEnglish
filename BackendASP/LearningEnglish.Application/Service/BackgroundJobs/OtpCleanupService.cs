using LearningEnglish.Application.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service.BackgroundJobs
{
    public class OtpCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<OtpCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(30); // Chạy mỗi 30 phút

        public OtpCleanupService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<OtpCleanupService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OTP Cleanup Service đã khởi động. Sẽ chạy mỗi {Interval} phút",
                _cleanupInterval.TotalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredOtpsAsync();

                    // Chờ 30 phút trước khi chạy lần tiếp theo
                    await Task.Delay(_cleanupInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Service đang dừng, không log error
                    _logger.LogInformation("OTP Cleanup Service đang dừng...");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi thực hiện cleanup OTP hết hạn");

                    // Chờ 5 phút trước khi retry nếu có lỗi
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("OTP Cleanup Service đã dừng");
        }

        private async Task CleanupExpiredOtpsAsync()
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var passwordResetTokenRepo = scope.ServiceProvider
                .GetRequiredService<IPasswordResetTokenRepository>();

            var emailVerificationTokenRepo = scope.ServiceProvider
                .GetRequiredService<IEmailVerificationTokenRepository>();

            var userRepository = scope.ServiceProvider
                .GetRequiredService<IUserRepository>();

            try
            {
                _logger.LogInformation("Bắt đầu cleanup OTP hết hạn và user chưa verify lúc {Time}", DateTime.UtcNow);

                // Xóa PasswordResetTokens hết hạn
                await passwordResetTokenRepo.DeleteExpiredTokensAsync();
                _logger.LogInformation("Đã xóa PasswordResetTokens hết hạn");

                // Xóa EmailVerificationTokens hết hạn
                await emailVerificationTokenRepo.DeleteExpiredTokensAsync();
                _logger.LogInformation("Đã xóa EmailVerificationTokens hết hạn");

                // XÓA USER CHƯA VERIFY EMAIL SAU 24 TIẾNG (BATCH PROCESSING)
                const int BATCH_SIZE = 100; // Xóa 100 users mỗi batch
                var cutoffTime = DateTime.UtcNow.AddHours(-24);
                int totalDeleted = 0;
                int skip = 0;
                bool hasMore = true;

                while (hasMore)
                {
                    // Load batch users từ database
                    var allUsers = await userRepository.GetAllUsersAsync();
                    var batch = allUsers.Where(u => 
                        !u.EmailVerified && 
                        u.CreatedAt < cutoffTime
                    ).Skip(skip).Take(BATCH_SIZE).ToList();

                    if (batch.Count == 0)
                    {
                        hasMore = false;
                        break;
                    }

                    try
                    {
                        foreach (var user in batch)
                        {
                            await userRepository.DeleteUserAsync(user.UserId);
                            _logger.LogInformation("Đã xóa user chưa verify: {Email} (Created: {CreatedAt})", 
                                user.Email, user.CreatedAt);
                        }

                        await userRepository.SaveChangesAsync();
                        totalDeleted += batch.Count;
                        _logger.LogInformation("✅ Batch completed: Đã xóa {Count} users chưa verify", batch.Count);
                    }
                    catch (Exception batchEx)
                    {
                        _logger.LogError(batchEx, "❌ Lỗi khi xóa batch users. Tiếp tục batch tiếp theo...");
                    }

                    skip += BATCH_SIZE;
                    
                    // Nếu batch nhỏ hơn BATCH_SIZE → đây là batch cuối
                    if (batch.Count < BATCH_SIZE)
                    {
                        hasMore = false;
                    }

                    // Throttle để tránh quá tải database
                    await Task.Delay(100);
                }

                if (totalDeleted > 0)
                {
                    _logger.LogInformation("✅ Hoàn thành xóa {Total} users chưa verify email sau 24 giờ", totalDeleted);
                }

                _logger.LogInformation("Hoàn thành cleanup lúc {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cleanup OTP và unverified users");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OTP Cleanup Service đang shutdown...");
            await base.StopAsync(stoppingToken);
        }
    }
}
