using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Interface.Strategies
{
    public interface IPaymentStrategy
    {
        ProductType ProductType { get; }
        
        // Xử lý business logic sau thanh toán (enroll, subscription, notification)
        Task<ServiceResponse<bool>> ProcessPostPaymentAsync(int userId, int productId, int paymentId);
        
        // Validate sản phẩm và trả về amount
        Task<ServiceResponse<decimal>> ValidateProductAsync(int productId);
        
        // Lấy tên sản phẩm
        Task<string> GetProductNameAsync(int productId);
    }
}
