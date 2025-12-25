using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Interface
{
    public interface IPaymentRepository
    {
        // Thêm thanh toán
        Task AddPaymentAsync(Payment payment);
        
        // Lấy thanh toán theo ID
        Task<Payment?> GetPaymentByIdAsync(int paymentId);
        
        // Lấy thanh toán của user
        Task<IEnumerable<Payment>> GetPaymentsByUserAsync(int userId);
        
        // Lấy thanh toán thành công cho khóa học
        Task<Payment?> GetSuccessfulPaymentByUserAndCourseAsync(int userId, int courseId);
        
        // Lấy thanh toán thành công cho sản phẩm
        Task<Payment?> GetSuccessfulPaymentByUserAndProductAsync(int userId, int productId, ProductType productType);
        
        // Lấy thanh toán theo IdempotencyKey (để prevent duplicate)
        Task<Payment?> GetPaymentByIdempotencyKeyAsync(int userId, string idempotencyKey);
        
        // Lấy thanh toán theo OrderCode (cho webhook processing)
        Task<Payment?> GetPaymentByOrderCodeAsync(long orderCode);
        
        // Cập nhật trạng thái thanh toán
        Task UpdatePaymentStatusAsync(Payment payment);
        
        // Lưu thay đổi
        Task<int> SaveChangesAsync();

        // Lấy lịch sử giao dịch với phân trang
        Task<IEnumerable<Payment>> GetTransactionHistoryAsync(int userId, int pageNumber, int pageSize);
        
        // Lấy tất cả lịch sử giao dịch

        
        // Đếm số giao dịch
        Task<int> GetTransactionCountAsync(int userId);
        
        // Lấy chi tiết giao dịch
        Task<Payment?> GetTransactionDetailAsync(int paymentId, int userId);
        
        // Lấy thanh toán theo transaction ID
        Task<Payment?> GetPaymentByTransactionIdAsync(string transactionId);
        
        // Lấy các payment Pending đã hết hạn (ExpiredAt < cutoffTime)
        Task<IEnumerable<Payment>> GetExpiredPendingPaymentsAsync(DateTime cutoffTime);
        
        // Revenue statistics methods
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetRevenueByStatusAsync(PaymentStatus status);
        Task<decimal> GetRevenueByDateRangeAsync(DateTime fromDate, DateTime? toDate = null);
        Task<int> GetTransactionsCountByStatusAsync(PaymentStatus status);
        Task<int> GetTransactionsCountByDateRangeAsync(DateTime fromDate, DateTime? toDate = null);
        
        // Revenue by ProductType (for chart breakdown)
        Task<decimal> GetRevenueByProductTypeAsync(ProductType productType);
        Task<decimal> GetRevenueByProductTypeAndDateRangeAsync(ProductType productType, DateTime fromDate, DateTime? toDate = null);
        
        // Revenue timeline (for chart data)
        Task<Dictionary<DateTime, decimal>> GetDailyRevenueAsync(DateTime fromDate, DateTime toDate);
        Task<Dictionary<DateTime, decimal>> GetMonthlyRevenueAsync(int year);
    }
}
