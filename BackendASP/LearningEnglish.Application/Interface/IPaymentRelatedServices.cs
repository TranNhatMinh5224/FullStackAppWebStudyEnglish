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
    
    public interface IPostPaymentProcessor
    {
        Task<ServiceResponse<bool>> ProcessCoursePaymentAsync(int userId, int courseId, int paymentId);
        Task<ServiceResponse<bool>> ProcessTeacherPackagePaymentAsync(int userId, int packageId, int paymentId);
    }
    
    public interface IPaymentNotificationService
    {
        Task SendCoursePaymentNotificationAsync(int userId, int courseId);
        Task SendTeacherPackagePaymentNotificationAsync(int userId, int packageId, DateTime validUntil);
    }
}
