using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class QuizSectionRepository : IQuizSectionRepository
    {
        private readonly AppDbContext _context;

        public QuizSectionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<QuizSection?> GetQuizSectionByIdAsync(int quizSectionId)
        {
            return await _context.QuizSections
                .Include(qs => qs.Quiz)
                .Include(qs => qs.QuizGroups)
                .FirstOrDefaultAsync(qs => qs.QuizSectionId == quizSectionId);
        }

        public async Task<List<QuizSection>> GetQuizSectionsByQuizIdAsync(int quizId)
        {
            return await _context.QuizSections
                .Include(qs => qs.QuizGroups)
                .Where(qs => qs.QuizId == quizId)
                .OrderBy(qs => qs.CreatedAt)
                .ToListAsync();
        }

        public async Task<QuizSection> CreateQuizSectionAsync(QuizSection quizSection)
        {
            quizSection.CreatedAt = DateTime.UtcNow;
            quizSection.UpdatedAt = DateTime.UtcNow;

            _context.QuizSections.Add(quizSection);
            await _context.SaveChangesAsync();

            return await GetQuizSectionByIdAsync(quizSection.QuizSectionId) ?? quizSection;
        }

        public async Task<QuizSection> UpdateQuizSectionAsync(QuizSection quizSection)
        {
            quizSection.UpdatedAt = DateTime.UtcNow;

            _context.QuizSections.Update(quizSection);
            await _context.SaveChangesAsync();

            return await GetQuizSectionByIdAsync(quizSection.QuizSectionId) ?? quizSection;
        }

        public async Task<bool> DeleteQuizSectionAsync(int quizSectionId)
        {
            var quizSection = await _context.QuizSections.FindAsync(quizSectionId);
            if (quizSection == null)
                return false;

            _context.QuizSections.Remove(quizSection);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> QuizSectionExistsAsync(int quizSectionId)
        {
            return await _context.QuizSections.AnyAsync(qs => qs.QuizSectionId == quizSectionId);
        }

        public async Task<Quiz?> GetQuizByIdAsync(int quizId)
        {
            return await _context.Quizzes.FindAsync(quizId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task AddQuizSectionAsync(QuizSection quizSection)
        {
            quizSection.CreatedAt = DateTime.UtcNow;
            quizSection.UpdatedAt = DateTime.UtcNow;
            _context.QuizSections.Add(quizSection);
        }
    }
}
