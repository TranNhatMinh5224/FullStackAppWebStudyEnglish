using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Interface
{
    public interface IPaymentValidator
    {
        
        // Validate sản phẩm (Course hoặc TeacherPackage) có tồn tại và lấy giá
    
        Task<ServiceResponse<decimal>> ValidateProductAsync(int productId, ProductType productType);

        
        // Validate user có thể mua sản phẩm này không (chưa mua trước đó)
    
        Task<ServiceResponse<bool>> ValidateUserPaymentAsync(int userId, int productId, ProductType productType);
    }
}

