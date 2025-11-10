using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Interface
{
    public interface IPaymentProcessorFactory
    {
        IPaymentProcessor GetProcessor(TypeProduct productType);
    }

    public interface IPaymentProcessor
    {
        Task<ServiceResponse<decimal>> ValidateProductAsync(int productId); // Validate product and return amount
        Task<ServiceResponse<bool>> ProcessPostPaymentAsync(int userId, int productId, int paymentId); // Process post-payment actions
    }
}
