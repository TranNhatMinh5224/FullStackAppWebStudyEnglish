using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class PronunciationAssessmentRepository : IPronunciationAssessmentRepository
    {
        private readonly AppDbContext _context;

        public PronunciationAssessmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PronunciationAssessment> CreateAsync(PronunciationAssessment assessment)
        {
            await _context.PronunciationAssessments.AddAsync(assessment);
            await _context.SaveChangesAsync();
            return assessment;
        }

        public async Task<PronunciationAssessment?> GetByIdAsync(int id)
        {
            return await _context.PronunciationAssessments
                .Include(p => p.User)
                .Include(p => p.FlashCard)
                .FirstOrDefaultAsync(p => p.PronunciationAssessmentId == id);
        }

        public async Task<List<PronunciationAssessment>> GetByUserIdAsync(int userId)
        {
            return await _context.PronunciationAssessments
                .Include(p => p.FlashCard)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PronunciationAssessment>> GetByFlashCardIdAsync(int flashCardId)
        {
            return await _context.PronunciationAssessments
                .Include(p => p.User)
                .Where(p => p.FlashCardId == flashCardId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PronunciationAssessment>> GetByAssignmentIdAsync(int assignmentId)
        {
            return await _context.PronunciationAssessments
                .Include(p => p.User)
                .Where(p => p.AssignmentId == assignmentId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateAsync(PronunciationAssessment assessment)
        {
            _context.PronunciationAssessments.Update(assessment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var assessment = await GetByIdAsync(id);
            if (assessment != null)
            {
                _context.PronunciationAssessments.Remove(assessment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> CountByUserIdAsync(int userId)
        {
            return await _context.PronunciationAssessments
                .Where(p => p.UserId == userId)
                .CountAsync();
        }
    }
}
