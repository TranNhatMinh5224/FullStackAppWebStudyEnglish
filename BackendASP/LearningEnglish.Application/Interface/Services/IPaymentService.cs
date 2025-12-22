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
        
        // Lấy tất cả lịch sử giao dịch
        Task<ServiceResponse<List<TransactionHistoryDto>>> GetAllTransactionHistoryAsync(int userId);
        
        // Lấy chi tiết giao dịch
        Task<ServiceResponse<TransactionDetailDto>> GetTransactionDetailAsync(int paymentId, int userId);
        
        // Tạo link thanh toán PayOS
        Task<ServiceResponse<PayOSLinkResponse>> CreatePayOSPaymentLinkAsync(int paymentId, int userId);
    }
}
