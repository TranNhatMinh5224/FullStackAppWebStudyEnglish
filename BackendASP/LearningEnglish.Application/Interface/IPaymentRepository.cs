using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Interface
{
    public interface IPaymentRepository
    {
        Task AddPaymentAsync(Payment payment);
        Task<Payment?> GetPaymentByIdAsync(int paymentId);
        Task<IEnumerable<Payment>> GetPaymentsByUserAsync(int userId);
        Task<Payment?> GetSuccessfulPaymentByUserAndCourseAsync(int userId, int courseId);
        Task<Payment?> GetSuccessfulPaymentByUserAndProductAsync(int userId, int productId, TypeProduct productType);
        Task UpdatePaymentStatusAsync(Payment payment);
        Task<int> SaveChangesAsync();
    }
}
