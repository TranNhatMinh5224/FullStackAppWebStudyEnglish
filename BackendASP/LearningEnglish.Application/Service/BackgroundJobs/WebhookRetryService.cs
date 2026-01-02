using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LearningEnglish.Application.Service.BackgroundJobs;

public class WebhookRetryService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<WebhookRetryService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Check mỗi 1 phút

    // Exponential backoff: 1min, 5min, 15min, 1h, 6h
    private readonly TimeSpan[] _retryDelays = new[]
    {
        TimeSpan.FromMinutes(1),   // Retry 1
        TimeSpan.FromMinutes(5),   // Retry 2
        TimeSpan.FromMinutes(15),  // Retry 3
        TimeSpan.FromHours(1),     // Retry 4
        TimeSpan.FromHours(6)      // Retry 5
    };

    public WebhookRetryService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<WebhookRetryService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🔄 Webhook Retry Service started. Checking every {Interval} minute(s)",
            _checkInterval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessWebhookRetries();
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Webhook Retry Service stopping...");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in Webhook Retry Service main loop");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("Webhook Retry Service stopped");
    }

    private async Task ProcessWebhookRetries()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var webhookRepository = scope.ServiceProvider.GetRequiredService<IPaymentWebhookQueueRepository>();
        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

        try
        {
            var currentTime = DateTime.UtcNow;

            // 1. Lấy pending webhooks (webhooks mới chưa xử lý)
            var pendingWebhooks = await webhookRepository.GetPendingWebhooksAsync();

            // 2. Lấy failed webhooks cần retry
            var failedWebhooks = await webhookRepository.GetFailedWebhooksForRetryAsync(currentTime);

            var totalWebhooks = pendingWebhooks.Count + failedWebhooks.Count;

            if (totalWebhooks == 0)
            {
                return; // Không có webhook nào cần xử lý
            }

            _logger.LogInformation("📨 Processing {Total} webhooks ({Pending} pending, {Failed} retries)",
                totalWebhooks, pendingWebhooks.Count, failedWebhooks.Count);

            // 3. Process pending webhooks trước
            foreach (var webhook in pendingWebhooks)
            {
                await ProcessWebhookAsync(webhook, webhookRepository, paymentService);
            }

            // 4. Process failed webhooks (retry)
            foreach (var webhook in failedWebhooks)
            {
                await ProcessWebhookAsync(webhook, webhookRepository, paymentService);
            }

            _logger.LogInformation("✅ Completed processing {Total} webhooks", totalWebhooks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing webhook retries");
        }
    }

    private async Task ProcessWebhookAsync(
        LearningEnglish.Domain.Entities.PaymentWebhookQueue webhook,
        IPaymentWebhookQueueRepository webhookRepository,
        IPaymentService paymentService)
    {
        try
        {
            _logger.LogInformation("🔄 Processing webhook {WebhookId} (Attempt {RetryCount}/{MaxRetries})",
                webhook.WebhookId, webhook.RetryCount + 1, webhook.MaxRetries);

            // Update status to Processing
            webhook.Status = WebhookStatus.Processing;
            webhook.LastAttemptAt = DateTime.UtcNow;
            await webhookRepository.UpdateWebhookStatusAsync(webhook);
            await webhookRepository.SaveChangesAsync();

            // Deserialize webhook data
            var webhookData = JsonSerializer.Deserialize<PayOSWebhookDto>(
                webhook.WebhookData);

            if (webhookData == null)
            {
                throw new Exception("Failed to deserialize webhook data");
            }

            // Process webhook using PaymentService (Skip Signature Check for retry)
            var result = await paymentService.ProcessWebhookFromQueueAsync(webhookData);

            if (!result.Success)
            {
                throw new Exception(result.Message ?? "Webhook processing failed");
            }

            // Mark as processed
            webhook.Status = WebhookStatus.Processed;
            webhook.ProcessedAt = DateTime.UtcNow;
            await webhookRepository.UpdateWebhookStatusAsync(webhook);
            await webhookRepository.SaveChangesAsync();

            _logger.LogInformation("✅ Webhook {WebhookId} processed successfully", webhook.WebhookId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to process webhook {WebhookId}", webhook.WebhookId);

            webhook.RetryCount++;
            webhook.LastError = ex.Message;
            webhook.ErrorStackTrace = ex.StackTrace;

            // Check if max retries reached
            if (webhook.RetryCount >= webhook.MaxRetries)
            {
                // Move to Dead Letter Queue
                webhook.Status = WebhookStatus.DeadLetter;
                webhook.NextRetryAt = null;
                _logger.LogError("💀 Webhook {WebhookId} moved to Dead Letter Queue after {RetryCount} attempts",
                    webhook.WebhookId, webhook.RetryCount);
            }
            else
            {
                // Schedule next retry with exponential backoff
                webhook.Status = WebhookStatus.Failed;
                var delayIndex = Math.Min(webhook.RetryCount - 1, _retryDelays.Length - 1);
                webhook.NextRetryAt = DateTime.UtcNow.Add(_retryDelays[delayIndex]);

                _logger.LogWarning("⏰ Webhook {WebhookId} scheduled for retry at {NextRetryAt}",
                    webhook.WebhookId, webhook.NextRetryAt);
            }

            await webhookRepository.UpdateWebhookStatusAsync(webhook);
            await webhookRepository.SaveChangesAsync();
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Webhook Retry Service shutdown initiated...");
        await base.StopAsync(stoppingToken);
    }
}