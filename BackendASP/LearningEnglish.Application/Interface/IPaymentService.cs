using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IPaymentService
    {
        Task<ServiceResponse<CreateInforPayment>> ProcessPaymentAsync(int userId, requestPayment dto);
        Task<ServiceResponse<bool>> ConfirmPaymentAsync(CompletePayment dto, int userId);
    }
}
