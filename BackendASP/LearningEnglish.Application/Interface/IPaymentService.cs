using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IPaymentService
    {
        Task<ServiceResponse<CreateInforPayment>> ProcessPaymentAsync(int userId, requestPayment dto);
        Task<ServiceResponse<bool>> ConfirmPaymentAsync(CompletePayment dto, int userId);
        Task<ServiceResponse<PagedResult<TransactionHistoryDto>>> GetTransactionHistoryAsync(int userId, int pageNumber, int pageSize);
        Task<ServiceResponse<TransactionDetailDto>> GetTransactionDetailAsync(int paymentId, int userId);
        Task<ServiceResponse<PayOSLinkResponse>> CreatePayOSPaymentLinkAsync(int paymentId, int userId);
    }
}
