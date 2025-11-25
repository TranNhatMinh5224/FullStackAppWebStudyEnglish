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

        public async Task UpdatePaymentStatusAsync(Payment payment)

        {
            _context.Payments.Update(payment);
            await Task.CompletedTask;

        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Transaction History
        public async Task<IEnumerable<Payment>> GetTransactionHistoryAsync(int userId, int pageNumber, int pageSize)
        {
            return await _context.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentId)
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

    }
}

