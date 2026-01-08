using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Interface;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Transactions;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null) // nếu đã có transaction thì không làm gì
            {
                return;
            }
            _transaction = await _context.Database.BeginTransactionAsync();
        }
        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await _context.SaveChangesAsync(ct);
        }

        public async Task CommitAsync()
        {
            if (_transaction == null) return;  // nếu không có transaction thì không làm gì

            await _context.Database.CommitTransactionAsync();
            await _context.SaveChangesAsync(); // lưu thay đổi trong transaction lưu để lấy luôn id nếu có
            await _transaction.DisposeAsync(); // giải phóng transaction
        }

        public async Task RollbackAsync()
        {

            if (_transaction == null) return;  // nếu không có transaction thì không làm gì
            await _context.Database.RollbackTransactionAsync();
            await _transaction.DisposeAsync(); // giải phóng transaction
            _transaction = null;

        }
    }
}
