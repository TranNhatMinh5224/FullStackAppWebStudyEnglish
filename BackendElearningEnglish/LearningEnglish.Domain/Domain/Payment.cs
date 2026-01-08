using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Domain.Entities;

public class Payment
{
    public int PaymentId { get; set; }
    public int UserId { get; set; }

    // Product information
    public ProductType ProductType { get; set; }
    public int ProductId { get; set; }

    // Payment details
    public long OrderCode { get; set; } // Unique order code for PayOS
    public string? IdempotencyKey { get; set; } // Unique key to prevent duplicate payments
    public decimal Amount { get; set; }
    public PaymentGateway Gateway { get; set; } = PaymentGateway.PayOs;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? Description { get; set; }

    // Provider information
    public string? ProviderTransactionId { get; set; } // Transaction ID from payment gateway
    public string? CheckoutUrl { get; set; } // Payment link URL
    public string? QrCode { get; set; } // QR code URL for payment

    // Banking information (for bank transfer)
    public string? AccountNumber { get; set; }
    public string? AccountName { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
    public DateTime? ExpiredAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Error tracking
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;
}
