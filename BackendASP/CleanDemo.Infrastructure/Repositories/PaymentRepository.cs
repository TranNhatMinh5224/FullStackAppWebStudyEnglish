using CleanDemo.Application.Interface;
using CleanDemo.Domain.Entities;
using CleanDemo.Domain.Enums;
using CleanDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanDemo.Infrastructure.Repositories
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
                .FirstOrDefaultAsync(p => p.UserId == userId && p.ProductId == courseId && p.ProductType == TypeProduct.Course && p.Status == PaymentStatus.Completed);
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

    }
}

