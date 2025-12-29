using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Interface.Strategies
{
    public interface IPaymentStrategy
    {
        ProductType ProductType { get; }
        
        // Xác thực sản phẩm
        Task<ServiceResponse<decimal>> ValidateProductAsync(int productId);
        
        // Lấy tên sản phẩm
        Task<string> GetProductNameAsync(int productId);
        
        // Xử lý sau thanh toán
        Task<ServiceResponse<bool>> ProcessPostPaymentAsync(int userId, int productId, int paymentId);
    }
}
