using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IPaymentService
    {
        // Xử lý thanh toán
        Task<ServiceResponse<CreateInforPayment>> ProcessPaymentAsync(int userId, requestPayment dto);
        
        // Xác nhận thanh toán
        Task<ServiceResponse<bool>> ConfirmPaymentAsync(CompletePayment dto, int userId);
        
        // Lấy lịch sử giao dịch phân trang
        Task<ServiceResponse<PagedResult<TransactionHistoryDto>>> GetTransactionHistoryAsync(int userId, PageRequest request);
        
        // Lấy chi tiết giao dịch
        Task<ServiceResponse<TransactionDetailDto>> GetTransactionDetailAsync(int paymentId, int userId);
        
        // Tạo link thanh toán PayOS
        Task<ServiceResponse<PayOSLinkResponse>> CreatePayOSPaymentLinkAsync(int paymentId, int userId);
        
        // Process webhook from queue (for retry mechanism)
        Task<ServiceResponse<bool>> ProcessWebhookFromQueueAsync(PayOSWebhookDto webhookData);
        
        // Xử lý PayOS webhook với signature verification
        Task<ServiceResponse<bool>> ProcessPayOSWebhookAsync(PayOSWebhookDto webhookData);
        
        // Xử lý PayOS return URL
        Task<ServiceResponse<PayOSReturnResult>> ProcessPayOSReturnAsync(string code, string desc, string data, string? orderCode = null);
        
        // Xác nhận thanh toán PayOS (với validation PayOS status)
        Task<ServiceResponse<bool>> ConfirmPayOSPaymentAsync(int paymentId, int userId);
    }
}
