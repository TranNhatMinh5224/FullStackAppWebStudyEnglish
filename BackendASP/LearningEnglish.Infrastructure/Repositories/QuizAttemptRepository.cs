using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class QuizAttemptRepository : IQuizAttemptRepository
    {
        private readonly AppDbContext _context;

        public QuizAttemptRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<QuizAttempt?> GetAttemptByIdAsync(int attemptId)
        {
            // KHÔNG dùng AsNoTracking() để EF Core tracking changes khi update answers
            return await _context.QuizAttempts
                .Include(a => a.Answers)
                    .ThenInclude(ua => ua.SelectedOptions)
                .FirstOrDefaultAsync(a => a.AttemptId == attemptId);
        }

        public async Task<QuizAttempt> CreateAttemptAsync(QuizAttempt attempt)
        {
            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();
            return attempt;
        }

        public async Task UpdateAttemptAsync(QuizAttempt attempt)
        {
            // EF Core sẽ tự động track changes từ GetAttemptByIdAsync
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UserHasActiveAttemptAsync(int quizId, int userId)
        {
            return await _context.QuizAttempts
                .AnyAsync(a => a.QuizId == quizId && a.UserId == userId && a.Status == QuizAttemptStatus.InProgress);
        }

        // Lấy attempt mới nhất (có AttemptNumber lớn nhất) để kiểm tra số lần làm bài
        public async Task<QuizAttempt?> GetLatestAttemptAsync(int quizId, int userId)
        {
            return await _context.QuizAttempts
                .Where(a => a.QuizId == quizId && a.UserId == userId)
                .OrderByDescending(a => a.AttemptNumber)
                .FirstOrDefaultAsync();
        }
    }
}
