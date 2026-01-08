using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface;

public interface IPaymentWebhookQueueRepository
{
    // Thêm webhook mới vào queue
    Task AddWebhookAsync(PaymentWebhookQueue webhook);
    
    // Lấy webhook theo ID
    Task<PaymentWebhookQueue?> GetWebhookByIdAsync(int webhookId);
    
    // Lấy webhooks đang pending (chưa xử lý)
    Task<List<PaymentWebhookQueue>> GetPendingWebhooksAsync();
    
    // Lấy webhooks failed cần retry (NextRetryAt <= now, RetryCount < MaxRetries)
    Task<List<PaymentWebhookQueue>> GetFailedWebhooksForRetryAsync(DateTime currentTime);
    
    // Update status và thông tin webhook
    Task UpdateWebhookStatusAsync(PaymentWebhookQueue webhook);
    
    // Lấy webhooks dead letter (để admin review)
    Task<List<PaymentWebhookQueue>> GetDeadLetterWebhooksAsync();
    
    // Save changes
    Task SaveChangesAsync();
}
