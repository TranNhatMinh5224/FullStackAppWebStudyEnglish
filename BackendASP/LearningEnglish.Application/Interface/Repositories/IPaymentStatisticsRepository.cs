using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Interface
{
    /// <summary>
    /// Repository cho Payment Statistics (tách riêng để dễ quản lý và maintain)
    /// </summary>
    public interface IPaymentStatisticsRepository
    {
        // Revenue statistics
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetRevenueByStatusAsync(PaymentStatus status);
        Task<decimal> GetRevenueByDateRangeAsync(DateTime fromDate, DateTime? toDate = null);
        
        // Transaction count statistics
        Task<int> GetTotalTransactionsCountAsync(); // Tất cả transactions (mọi status)
        Task<int> GetTransactionsCountByStatusAsync(PaymentStatus status);
        Task<int> GetTransactionsCountByDateRangeAsync(DateTime fromDate, DateTime? toDate = null);
        
        // Revenue by ProductType (for chart breakdown)
        Task<decimal> GetRevenueByProductTypeAsync(ProductType productType);
        Task<decimal> GetRevenueByProductTypeAndDateRangeAsync(ProductType productType, DateTime fromDate, DateTime? toDate = null);
        
        // Revenue timeline (for chart data)
        Task<Dictionary<DateTime, decimal>> GetDailyRevenueAsync(DateTime fromDate, DateTime toDate);
        Task<Dictionary<DateTime, decimal>> GetDailyRevenueByProductTypeAsync(ProductType productType, DateTime fromDate, DateTime toDate);
        Task<Dictionary<DateTime, decimal>> GetMonthlyRevenueAsync(int year);
    }
}

