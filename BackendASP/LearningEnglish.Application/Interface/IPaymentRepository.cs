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
        Task<Payment?> GetSuccessfulPaymentByUserAndProductAsync(int userId, int productId, ProductType productType);
        Task UpdatePaymentStatusAsync(Payment payment);
        Task<int> SaveChangesAsync();

        // Transaction History
        Task<IEnumerable<Payment>> GetTransactionHistoryAsync(int userId, int pageNumber, int pageSize);
        Task<IEnumerable<Payment>> GetAllTransactionHistoryAsync(int userId);
        Task<int> GetTransactionCountAsync(int userId);
        Task<Payment?> GetTransactionDetailAsync(int paymentId, int userId);
        
        // PayOS
        Task<Payment?> GetPaymentByTransactionIdAsync(string transactionId);
    }
}
