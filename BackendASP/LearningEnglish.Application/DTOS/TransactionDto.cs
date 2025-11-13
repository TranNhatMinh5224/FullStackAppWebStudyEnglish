using System;

using LearningEnglish.Domain.Enums;
namespace LearningEnglish.Application.DTOs
{
    public class TransactionHistoryDto
    {
        public string PaymentMethod { get; set; } = string.Empty;

        public ProductType ProductType { get; set; }
        public int ProductId { get; set; }

        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }

        public DateTime? PaidAt { get; set; }
        public string? ProviderTransactionId { get; set; }

    }
    public class GetPaymentById
    {
        public int PaymentId { get; set; }
        public int UserId { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;

        public ProductType ProductType { get; set; }
        public int ProductId { get; set; }

        public decimal Amount { get; set; }
        public PaymentStatus Status
        {
            get; set;
        }

        public DateTime? PaidAt { get; set; }
        public string? ProviderTransactionId { get; set; }

    }
}
