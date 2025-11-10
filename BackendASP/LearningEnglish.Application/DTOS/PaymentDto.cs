

using LearningEnglish.Domain.Enums;
namespace LearningEnglish.Application.DTOs
{
    public class requestPayment
    {
        public int ProductId { get; set; }
        public TypeProduct typeproduct { get; set; }

    }
    // tạo thông tin thanh toán trả về cho client
    public class CreateInforPayment
    {
        public int PaymentId { get; set; }
        public TypeProduct ProductType { get; set; }
        public int ProductId { get; set; }

        public decimal Amount { get; set; }


    }
    // DTO cho thông tin thanh toán client trả cho server
    public class CompletePayment
    {
        public int PaymentId { get; set; }

        public int ProductId { get; set; }
        public TypeProduct ProductType { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;

    }
}

