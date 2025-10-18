using CleanDemo.Domain.Entities;
using CleanDemo.Domain.Enums;

namespace CleanDemo.Application.Interface
{
    public interface IPaymentRepository
    {
        Task AddPaymentAsync(Payment payment);
        Task<Payment?> GetPaymentByIdAsync(int paymentId);
        Task<IEnumerable<Payment>> GetPaymentsByUserAsync(int userId);
        Task UpdatePaymentStatusAsync(int paymentId, PaymentStatus status);
    }
}
