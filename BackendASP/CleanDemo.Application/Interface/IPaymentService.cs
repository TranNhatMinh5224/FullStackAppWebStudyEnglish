using CleanDemo.Application.Common;
using CleanDemo.Application.DTOs;

namespace CleanDemo.Application.Interface
{
    public interface IPaymentService
    {
        Task<ServiceResponse<CreateInforPayment>> ProcessPaymentAsync(int userId, requestPayment dto);
        Task<ServiceResponse<bool>> ConfirmPaymentAsync(CompletePayment dto, int userId);
    }
}
