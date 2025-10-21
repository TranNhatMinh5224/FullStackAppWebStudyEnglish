namespace CleanDemo.Application.Interface
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync(); // bắt đầu giao dịch
        Task<int> SaveChangesAsync(CancellationToken ct = default); // lưu thay đổi nhưng chưa commit giao dịch
        Task CommitAsync(); // xác nhận giao dịch
        Task RollbackAsync(); // hoàn tác giao dịch
    }
}

