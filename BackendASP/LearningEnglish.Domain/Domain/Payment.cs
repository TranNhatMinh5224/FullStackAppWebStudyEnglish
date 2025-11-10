using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Domain.Entities;

public class Payment
{
    public int PaymentId { get; set; }
    public int UserId { get; set; }

    public string? PaymentMethod { get; set; } = string.Empty;

    public TypeProduct ProductType { get; set; }
    public int ProductId { get; set; }

    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public DateTime? PaidAt { get; set; }
    public string? ProviderTransactionId { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;

}
