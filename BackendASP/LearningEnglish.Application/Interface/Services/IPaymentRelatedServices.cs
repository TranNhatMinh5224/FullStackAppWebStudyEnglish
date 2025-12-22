using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Interface
{
    public interface IPaymentValidator
    {
        Task<ServiceResponse<decimal>> ValidateProductAsync(int productId, ProductType productType);
        Task<ServiceResponse<bool>> ValidateUserPaymentAsync(int userId, int productId, ProductType productType);
    }
}
