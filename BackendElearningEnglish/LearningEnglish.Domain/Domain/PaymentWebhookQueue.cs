using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Domain.Entities;

public class PaymentWebhookQueue
{
    public int WebhookId { get; set; }
    
    // Payment reference
    public int? PaymentId { get; set; }
    public long OrderCode { get; set; }
    
    // Webhook data
    public string WebhookData { get; set; } = string.Empty; // JSON payload from PayOS
    public string? Signature { get; set; }
    
    // Status tracking
    public WebhookStatus Status { get; set; } = WebhookStatus.Pending;
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 5;
    
    // Timing
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public DateTime? NextRetryAt { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    
    // Error tracking
    public string? LastError { get; set; }
    public string? ErrorStackTrace { get; set; }
    
    // Relationships
    public Payment? Payment { get; set; }
}
