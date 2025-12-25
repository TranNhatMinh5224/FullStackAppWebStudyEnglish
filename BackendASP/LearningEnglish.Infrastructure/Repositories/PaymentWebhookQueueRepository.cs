using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories;

public class PaymentWebhookQueueRepository : IPaymentWebhookQueueRepository
{
    private readonly AppDbContext _context;

    public PaymentWebhookQueueRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddWebhookAsync(PaymentWebhookQueue webhook)
    {
        await _context.PaymentWebhookQueues.AddAsync(webhook);
    }

    public async Task<PaymentWebhookQueue?> GetWebhookByIdAsync(int webhookId)
    {
        return await _context.PaymentWebhookQueues
            .Include(w => w.Payment)
            .FirstOrDefaultAsync(w => w.WebhookId == webhookId);
    }

    public async Task<List<PaymentWebhookQueue>> GetPendingWebhooksAsync()
    {
        return await _context.PaymentWebhookQueues
            .Where(w => w.Status == WebhookStatus.Pending)
            .OrderBy(w => w.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<PaymentWebhookQueue>> GetFailedWebhooksForRetryAsync(DateTime currentTime)
    {
        return await _context.PaymentWebhookQueues
            .Where(w => 
                w.Status == WebhookStatus.Failed && 
                w.RetryCount < w.MaxRetries &&
                w.NextRetryAt.HasValue &&
                w.NextRetryAt.Value <= currentTime)
            .OrderBy(w => w.NextRetryAt)
            .ToListAsync();
    }

    public async Task UpdateWebhookStatusAsync(PaymentWebhookQueue webhook)
    {
        _context.PaymentWebhookQueues.Update(webhook);
    }

    public async Task<List<PaymentWebhookQueue>> GetDeadLetterWebhooksAsync()
    {
        return await _context.PaymentWebhookQueues
            .Where(w => w.Status == WebhookStatus.DeadLetter)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
