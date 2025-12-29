using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Domain.Entities;
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
        public async Task AddQuizAttemptAsync(QuizAttempt attempt)
        {
            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();
        }
        public async Task<QuizAttempt?> GetByIdAsync(int attemptId)
        {
            return await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                .Include(qa => qa.User)
                .FirstOrDefaultAsync(qa => qa.AttemptId == attemptId);
        }

        public async Task<QuizAttempt?> GetByIdAndUserIdAsync(int attemptId, int userId)
        {
            return await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                .Include(qa => qa.User)
                .FirstOrDefaultAsync(qa => qa.AttemptId == attemptId && qa.UserId == userId);
        }

        public async Task UpdateQuizAttemptAsync(QuizAttempt attempt)
        {
            _context.QuizAttempts.Update(attempt);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteQuizAttemptAsync(int attemptId)
        {
            var attempt = await _context.QuizAttempts.FindAsync(attemptId);
            if (attempt != null)
            {
                _context.QuizAttempts.Remove(attempt);
                await _context.SaveChangesAsync();
            }
        }
        // Lấy attempts 
        public async Task<List<QuizAttempt>> GetByUserAndQuizAsync(int userId, int quizId)
        {
            return await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                .Include(qa => qa.User)
                .Where(qa => qa.UserId == userId && qa.QuizId == quizId)
                .OrderByDescending(qa => qa.StartedAt)
                .ToListAsync();

        }
        public async Task<QuizAttempt?> GetActiveAttemptAsync(int userId, int quizId)
        {
            return await _context.QuizAttempts
               .Include(qa => qa.Quiz)
                .Include(qa => qa.User)
                .Where(qa => qa.UserId == userId && qa.QuizId == quizId && qa.Status == QuizAttemptStatus.InProgress)
                .FirstOrDefaultAsync();

        }

        public async Task<List<QuizAttempt>> GetInProgressAttemptsAsync()
        {
            return await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                .Where(qa => qa.Status == QuizAttemptStatus.InProgress)
                .ToListAsync();
        }

        public async Task<List<QuizAttempt>> GetByQuizIdAsync(int quizId)
        {
            return await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                .Include(qa => qa.User)
                .Where(qa => qa.QuizId == quizId)
                .OrderByDescending(qa => qa.StartedAt)
                .ToListAsync();
        }

        public async Task<List<QuizAttempt>> GetSubmittedAttemptsByQuizIdAsync(int quizId)
        {
            return await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                .Include(qa => qa.User)
                .Where(qa => qa.QuizId == quizId && qa.Status == QuizAttemptStatus.Submitted)
                .ToListAsync();
        }

        // Lấy danh sách attempts với phân trang
        public async Task<PagedResult<QuizAttempt>> GetQuizAttemptsPagedAsync(int quizId, QuizAttemptQueryParameters request)
        {
            var query = _context.QuizAttempts
                .Include(qa => qa.Quiz)
                .Include(qa => qa.User)
                .Where(qa => qa.QuizId == quizId)
                .OrderByDescending(qa => qa.StartedAt)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(qa =>
                    (qa.User.FirstName + " " + qa.User.LastName).Contains(request.SearchTerm) ||
                    qa.User.Email.Contains(request.SearchTerm));
            }

            return await query.ToPagedListAsync(request.PageNumber, request.PageSize);
        }

        // Lấy điểm của học sinh với phân trang
        public async Task<PagedResult<QuizAttempt>> GetQuizScoresPagedAsync(int quizId, QuizAttemptQueryParameters request)
        {
            var query = _context.QuizAttempts
                .Include(qa => qa.Quiz)
                .Include(qa => qa.User)
                .Where(qa => qa.QuizId == quizId && qa.Status == QuizAttemptStatus.Submitted)
                .OrderByDescending(qa => qa.TotalScore)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(qa =>
                    (qa.User.FirstName + " " + qa.User.LastName).Contains(request.SearchTerm) ||
                    qa.User.Email.Contains(request.SearchTerm));
            }

            return await query.ToPagedListAsync(request.PageNumber, request.PageSize);
        }

        public async Task<List<QuizAttempt>> GetQuizScoresAsync(int quizId)
        {
            return await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                .Include(qa => qa.User)
                .Where(qa => qa.QuizId == quizId && qa.Status == QuizAttemptStatus.Submitted)
                .OrderByDescending(qa => qa.TotalScore)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
