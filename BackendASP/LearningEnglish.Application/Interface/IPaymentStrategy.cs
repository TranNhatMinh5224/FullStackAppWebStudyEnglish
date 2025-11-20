using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Interface.Strategies
{
    public interface IPaymentStrategy
    {
        ProductType ProductType { get; }
        Task<ServiceResponse<decimal>> ValidateProductAsync(int productId);
        Task<ServiceResponse<bool>> ProcessPostPaymentAsync(int userId, int productId, int paymentId);
    }
}
