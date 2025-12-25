using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.API.Controller.AdminAndTeacher;

[ApiController]
[Route("api/admin/webhooks")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class AdminWebhookController : ControllerBase
{
    private readonly IPaymentWebhookQueueRepository _webhookRepository;
    private readonly ILogger<AdminWebhookController> _logger;

    public AdminWebhookController(
        IPaymentWebhookQueueRepository webhookRepository,
        ILogger<AdminWebhookController> logger)
    {
        _webhookRepository = webhookRepository;
        _logger = logger;
    }

    // GET: api/admin/webhooks/dead-letter - Lấy webhooks failed cần xem xét
    [HttpGet("dead-letter")]
    public async Task<IActionResult> GetDeadLetterWebhooks()
    {
        try
        {
            var webhooks = await _webhookRepository.GetDeadLetterWebhooksAsync();
            
            var result = webhooks.Select(w => new
            {
                w.WebhookId,
                w.PaymentId,
                w.OrderCode,
                w.Status,
                w.RetryCount,
                w.CreatedAt,
                w.LastAttemptAt,
                w.LastError,
                ErrorPreview = w.LastError?.Length > 200 ? w.LastError.Substring(0, 200) + "..." : w.LastError
            });

            return Ok(new
            {
                success = true,
                count = webhooks.Count,
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dead letter webhooks");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    // GET: api/admin/webhooks/{webhookId} - Chi tiết webhook
    [HttpGet("{webhookId}")]
    public async Task<IActionResult> GetWebhookDetail(int webhookId)
    {
        try
        {
            var webhook = await _webhookRepository.GetWebhookByIdAsync(webhookId);
            
            if (webhook == null)
            {
                return NotFound(new { success = false, message = "Webhook not found" });
            }

            return Ok(new
            {
                success = true,
                data = new
                {
                    webhook.WebhookId,
                    webhook.PaymentId,
                    webhook.OrderCode,
                    webhook.WebhookData,
                    webhook.Signature,
                    webhook.Status,
                    webhook.RetryCount,
                    webhook.MaxRetries,
                    webhook.CreatedAt,
                    webhook.ProcessedAt,
                    webhook.NextRetryAt,
                    webhook.LastAttemptAt,
                    webhook.LastError,
                    webhook.ErrorStackTrace,
                    Payment = webhook.Payment != null ? new
                    {
                        webhook.Payment.PaymentId,
                        webhook.Payment.Status,
                        webhook.Payment.Amount,
                        webhook.Payment.ProductType,
                        webhook.Payment.CreatedAt
                    } : null
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting webhook detail {WebhookId}", webhookId);
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    // POST: api/admin/webhooks/{webhookId}/retry - Manual retry webhook
    [HttpPost("{webhookId}/retry")]
    public async Task<IActionResult> ManualRetryWebhook(int webhookId)
    {
        try
        {
            var webhook = await _webhookRepository.GetWebhookByIdAsync(webhookId);
            
            if (webhook == null)
            {
                return NotFound(new { success = false, message = "Webhook not found" });
            }

            if (webhook.Status == LearningEnglish.Domain.Enums.WebhookStatus.Processed)
            {
                return BadRequest(new { success = false, message = "Webhook already processed" });
            }

            // Reset for retry
            webhook.Status = LearningEnglish.Domain.Enums.WebhookStatus.Pending;
            webhook.RetryCount = 0;
            webhook.NextRetryAt = DateTime.UtcNow;
            webhook.LastError = "Manual retry by admin";

            await _webhookRepository.UpdateWebhookStatusAsync(webhook);
            await _webhookRepository.SaveChangesAsync();

            _logger.LogInformation("Admin manually reset webhook {WebhookId} for retry", webhookId);

            return Ok(new
            {
                success = true,
                message = "Webhook reset for retry. It will be processed in the next cycle."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error manually retrying webhook {WebhookId}", webhookId);
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    // GET: api/admin/webhooks/stats - Webhook statistics
    [HttpGet("stats")]
    public async Task<IActionResult> GetWebhookStats()
    {
        try
        {
            var pendingWebhooks = await _webhookRepository.GetPendingWebhooksAsync();
            var failedWebhooks = await _webhookRepository.GetFailedWebhooksForRetryAsync(DateTime.UtcNow.AddHours(24));
            var deadLetterWebhooks = await _webhookRepository.GetDeadLetterWebhooksAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    pending = pendingWebhooks.Count,
                    failed = failedWebhooks.Count,
                    deadLetter = deadLetterWebhooks.Count,
                    total = pendingWebhooks.Count + failedWebhooks.Count + deadLetterWebhooks.Count
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting webhook stats");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }
}
