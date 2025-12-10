using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IPayOSService
    {
        Task<ServiceResponse<PayOSLinkResponse>> CreatePaymentLinkAsync(
            CreatePayOSLinkRequest request, 
            decimal amount, 
            string productName, 
            string description);
        
        Task<ServiceResponse<PayOSWebhookDto>> GetPaymentInformationAsync(long orderCode);
        
        Task<bool> VerifyWebhookSignature(string data, string signature);
    }
}

