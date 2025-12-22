using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IPayOSService
    {
        // Tạo link thanh toán
        Task<ServiceResponse<PayOSLinkResponse>> CreatePaymentLinkAsync(
            CreatePayOSLinkRequest request, 
            decimal amount, 
            string productName, 
            string description);
        
        // Lấy thông tin thanh toán
        Task<ServiceResponse<PayOSWebhookDto>> GetPaymentInformationAsync(long orderCode);
        
        // Xác thực webhook signature
        Task<bool> VerifyWebhookSignature(string data, string signature);
    }
}

