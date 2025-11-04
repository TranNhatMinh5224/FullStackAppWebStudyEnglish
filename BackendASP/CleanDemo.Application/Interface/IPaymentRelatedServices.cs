using CleanDemo.Application.Common;
using CleanDemo.Application.DTOs;
using CleanDemo.Domain.Enums;

namespace CleanDemo.Application.Interface
{
    public interface IPaymentValidator
    {
        Task<ServiceResponse<decimal>> ValidateProductAsync(int productId, TypeProduct productType);
        Task<ServiceResponse<bool>> ValidateUserPaymentAsync(int userId, int productId, TypeProduct productType);
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
