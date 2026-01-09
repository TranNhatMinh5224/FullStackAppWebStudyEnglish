using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface.Services
{
    public interface IPaymentService
    {
        /// <summary>
        /// Tạo payment record (POST /api/payments)
        /// </summary>
        Task<ServiceResponse<CreateInforPayment>> ProcessPaymentAsync(int userId, requestPayment request);

        /// <summary>
        /// Tạo PayOS payment link (POST /api/payments/{paymentId}/payos)
        /// </summary>
        Task<ServiceResponse<PayOSLinkResponse>> CreatePayOSPaymentLinkAsync(int paymentId, int userId);

        /// <summary>
        /// Confirm payment thủ công (POST /api/payments/confirm)
        /// </summary>
        Task<ServiceResponse<bool>> ConfirmPaymentAsync(CompletePayment paymentDto, int userId);

        /// <summary>
        /// Xử lý PayOS webhook với signature verification (POST /api/payments/payos/webhook)
        /// </summary>
        Task<ServiceResponse<bool>> ProcessPayOSWebhookAsync(PayOSWebhookDto webhookData);

        /// <summary>
        /// Xử lý webhook từ queue (retry mechanism) - không cần signature
        /// </summary>
        Task<ServiceResponse<bool>> ProcessWebhookFromQueueAsync(PayOSWebhookDto webhookData);

        /// <summary>
        /// Confirm PayOS payment với validation PayOS status (GET /api/payments/payos/confirm/{paymentId})
        /// </summary>
        Task<ServiceResponse<bool>> ConfirmPayOSPaymentAsync(int paymentId, int userId);

        /// <summary>
        /// Xử lý PayOS return URL (GET /api/payments/payos/return)
        /// </summary>
        Task<ServiceResponse<PayOSReturnResult>> ProcessPayOSReturnAsync(
            string code, 
            string desc, 
            string data, 
            string? orderCode = null, 
            string? status = null);

        /// <summary>
        /// Lấy lịch sử giao dịch với phân trang (GET /api/payments/history)
        /// </summary>
        Task<ServiceResponse<PagedResult<TransactionHistoryDto>>> GetTransactionHistoryAsync(int userId, PageRequest request);

        /// <summary>
        /// Lấy chi tiết giao dịch (GET /api/payments/{paymentId})
        /// </summary>
        Task<ServiceResponse<TransactionDetailDto>> GetTransactionDetailAsync(int paymentId, int userId);
    }
}
