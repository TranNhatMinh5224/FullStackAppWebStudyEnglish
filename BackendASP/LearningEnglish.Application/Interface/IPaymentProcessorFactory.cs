using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Interface
{
    public interface IPaymentProcessorFactory
    {
        IPaymentProcessor GetProcessor(ProductType productType); // Lấy bộ xử lý thanh toán dựa trên loại sản phẩm
    }

    public interface IPaymentProcessor
    {
        Task<ServiceResponse<decimal>> ValidateProductAsync(int productId); //xác nhận sản phẩm và trả về số tiền
        Task<ServiceResponse<bool>> ProcessPostPaymentAsync(int userId, int productId, int paymentId); //  xử lý các hành động sau khi thanh toán
    }
}
