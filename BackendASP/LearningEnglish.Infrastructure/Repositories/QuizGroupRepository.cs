using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class QuizGroupRepository : IQuizGroupRepository
    {
        private readonly AppDbContext _context;

        public QuizGroupRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<QuizGroup?> GetQuizGroupByIdAsync(int quizGroupId)
        {
            return await _context.QuizGroups
                .Include(qg => qg.QuizSection)
                .Include(qg => qg.Questions)
                .FirstOrDefaultAsync(qg => qg.QuizGroupId == quizGroupId);
        }

        public async Task<List<QuizGroup>> GetQuizGroupsByQuizSectionIdAsync(int quizSectionId)
        {
            return await _context.QuizGroups
                .Include(qg => qg.Questions)
                .Where(qg => qg.QuizSectionId == quizSectionId)
                .OrderBy(qg => qg.CreatedAt)
                .ToListAsync();
        }

        public async Task<QuizGroup> CreateQuizGroupAsync(QuizGroup quizGroup)
        {
            quizGroup.CreatedAt = DateTime.UtcNow;
            quizGroup.UpdatedAt = DateTime.UtcNow;

            _context.QuizGroups.Add(quizGroup);
            await _context.SaveChangesAsync();

            return await GetQuizGroupByIdAsync(quizGroup.QuizGroupId) ?? quizGroup;
        }

        public async Task<QuizGroup> UpdateQuizGroupAsync(QuizGroup quizGroup)
        {
            quizGroup.UpdatedAt = DateTime.UtcNow;

            _context.QuizGroups.Update(quizGroup);
            await _context.SaveChangesAsync();

            return await GetQuizGroupByIdAsync(quizGroup.QuizGroupId) ?? quizGroup;
        }

        public async Task<bool> DeleteQuizGroupAsync(int quizGroupId)
        {
            var quizGroup = await _context.QuizGroups.FindAsync(quizGroupId);
            if (quizGroup == null)
                return false;

            _context.QuizGroups.Remove(quizGroup);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> QuizGroupExistsAsync(int quizGroupId)
        {
            return await _context.QuizGroups.AnyAsync(qg => qg.QuizGroupId == quizGroupId);
        }

        public async Task<QuizSection?> GetQuizSectionByIdAsync(int quizSectionId)
        {
            return await _context.QuizSections.FindAsync(quizSectionId);
        }
    }
}
