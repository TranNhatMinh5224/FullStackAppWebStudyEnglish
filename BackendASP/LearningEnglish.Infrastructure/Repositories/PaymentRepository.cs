using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddPaymentAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);

        }

        public async Task<Payment?> GetPaymentByIdAsync(int paymentId)
        {
            return await _context.Payments
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByUserAsync(int userId)
        {
            return await _context.Payments
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<Payment?> GetSuccessfulPaymentByUserAndCourseAsync(int userId, int courseId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.UserId == userId && p.ProductId == courseId && p.ProductType == ProductType.Course && p.Status == PaymentStatus.Completed);
        }

        public async Task<Payment?> GetSuccessfulPaymentByUserAndProductAsync(int userId, int productId, ProductType productType)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.UserId == userId && p.ProductId == productId && p.ProductType == productType && p.Status == PaymentStatus.Completed);
        }

        public async Task<Payment?> GetPaymentByIdempotencyKeyAsync(int userId, string idempotencyKey)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.UserId == userId && p.IdempotencyKey == idempotencyKey);
        }

        public async Task<Payment?> GetPaymentByOrderCodeAsync(long orderCode)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderCode == orderCode);
        }

        public async Task UpdatePaymentStatusAsync(Payment payment)

        {
            _context.Payments.Update(payment);
            await Task.CompletedTask;

        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Transaction History - Giao dịch mới nhất lên đầu
        public async Task<IEnumerable<Payment>> GetTransactionHistoryAsync(int userId, int pageNumber, int pageSize)
        {
            return await _context.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaidAt ?? DateTime.MinValue)  // Sort by PaidAt DESC (mới nhất lên đầu)
                .ThenByDescending(p => p.PaymentId)  // Nếu PaidAt null thì sort theo PaymentId
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTransactionCountAsync(int userId)
        {
            return await _context.Payments
                .Where(p => p.UserId == userId)
                .CountAsync();
        }

        public async Task<Payment?> GetTransactionDetailAsync(int paymentId, int userId)
        {
            return await _context.Payments
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId && p.UserId == userId);
        }

        public async Task<Payment?> GetPaymentByTransactionIdAsync(string transactionId)
        {
            return await _context.Payments
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.ProviderTransactionId == transactionId);
        }

        public async Task<IEnumerable<Payment>> GetExpiredPendingPaymentsAsync(DateTime cutoffTime)
        {
            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Pending && 
                           p.ExpiredAt.HasValue && 
                           p.ExpiredAt.Value < cutoffTime)
                .ToListAsync();
        }

        // Revenue statistics methods
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

        // Revenue by ProductType
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

        // Revenue timeline for chart
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

