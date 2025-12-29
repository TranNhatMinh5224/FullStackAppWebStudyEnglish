using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories
{

    public class PaymentStatisticsRepository : IPaymentStatisticsRepository
    {
        private readonly AppDbContext _context;

        public PaymentStatisticsRepository(AppDbContext context)
        {
            _context = context;
        }

        // Revenue statistics
        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .SumAsync(p => p.Amount);
        }

        public async Task<decimal> GetRevenueByStatusAsync(PaymentStatus status)
        {
            return await _context.Payments
                .Where(p => p.Status == status)
                .SumAsync(p => p.Amount);
        }

        public async Task<decimal> GetRevenueByDateRangeAsync(DateTime fromDate, DateTime? toDate = null)
        {
            var query = _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed && p.PaidAt >= fromDate);

            if (toDate.HasValue)
            {
                query = query.Where(p => p.PaidAt <= toDate.Value);
            }

            return await query.SumAsync(p => p.Amount);
        }

        // Transaction count statistics
        public async Task<int> GetTotalTransactionsCountAsync()
        {
            return await _context.Payments.CountAsync();
        }

        public async Task<int> GetTransactionsCountByStatusAsync(PaymentStatus status)
        {
            return await _context.Payments
                .Where(p => p.Status == status)
                .CountAsync();
        }

        public async Task<int> GetTransactionsCountByDateRangeAsync(DateTime fromDate, DateTime? toDate = null)
        {
            var query = _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed && p.PaidAt >= fromDate);

            if (toDate.HasValue)
            {
                query = query.Where(p => p.PaidAt <= toDate.Value);
            }

            return await query.CountAsync();
        }

        // Revenue by ProductType (for chart breakdown)
        public async Task<decimal> GetRevenueByProductTypeAsync(ProductType productType)
        {
            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed && p.ProductType == productType)
                .SumAsync(p => p.Amount);
        }

        public async Task<decimal> GetRevenueByProductTypeAndDateRangeAsync(ProductType productType, DateTime fromDate, DateTime? toDate = null)
        {
            var query = _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed && p.ProductType == productType && p.PaidAt >= fromDate);

            if (toDate.HasValue)
            {
                query = query.Where(p => p.PaidAt <= toDate.Value);
            }

            return await query.SumAsync(p => p.Amount);
        }

        // Revenue timeline (for chart data)
        public async Task<Dictionary<DateTime, decimal>> GetDailyRevenueAsync(DateTime fromDate, DateTime toDate)
        {
            var payments = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed && 
                           p.PaidAt >= fromDate && 
                           p.PaidAt <= toDate)
                .Select(p => new { Date = p.PaidAt!.Value.Date, p.Amount })
                .ToListAsync();

            return payments
                .GroupBy(p => p.Date)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));
        }

        public async Task<Dictionary<DateTime, decimal>> GetDailyRevenueByProductTypeAsync(ProductType productType, DateTime fromDate, DateTime toDate)
        {
            var payments = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed && 
                           p.ProductType == productType &&
                           p.PaidAt >= fromDate && 
                           p.PaidAt <= toDate)
                .Select(p => new { Date = p.PaidAt!.Value.Date, p.Amount })
                .ToListAsync();

            return payments
                .GroupBy(p => p.Date)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));
        }

        public async Task<Dictionary<DateTime, decimal>> GetMonthlyRevenueAsync(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31, 23, 59, 59);

            var payments = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed && 
                           p.PaidAt >= startDate && 
                           p.PaidAt <= endDate)
                .Select(p => new { 
                    Year = p.PaidAt!.Value.Year, 
                    Month = p.PaidAt!.Value.Month, 
                    p.Amount 
                })
                .ToListAsync();

            return payments
                .GroupBy(p => new DateTime(p.Year, p.Month, 1))
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));
        }
    }
}

