namespace LearningEnglish.Application.Interface
{
    public interface IUnitOfWork
    {
        // Bắt đầu giao dịch
        Task BeginTransactionAsync();
        
        // Lưu thay đổi
        Task<int> SaveChangesAsync(CancellationToken ct = default);
        
        // Xác nhận giao dịch
        Task CommitAsync();
        
        // Hoàn tác giao dịch
        Task RollbackAsync();
    }
}

