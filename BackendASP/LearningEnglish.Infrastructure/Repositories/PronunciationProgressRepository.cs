using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class PronunciationProgressRepository : IPronunciationProgressRepository
    {
        private readonly AppDbContext _context;

        public PronunciationProgressRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PronunciationProgress?> GetByIdAsync(int id)
        {
            return await _context.PronunciationProgresses
                .Include(p => p.User)
                .Include(p => p.FlashCard)
                .FirstOrDefaultAsync(p => p.PronunciationProgressId == id);
        }

        public async Task<PronunciationProgress?> GetByUserAndFlashCardAsync(int userId, int flashCardId)
        {
            return await _context.PronunciationProgresses
                .Include(p => p.FlashCard)
                .FirstOrDefaultAsync(p => p.UserId == userId && p.FlashCardId == flashCardId);
        }

        public async Task<List<PronunciationProgress>> GetByUserIdAsync(int userId)
        {
            return await _context.PronunciationProgresses
                .Include(p => p.FlashCard)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.UpdatedAt)
                .ToListAsync();
        }

        public async Task<List<PronunciationProgress>> GetByFlashCardIdAsync(int flashCardId)
        {
            return await _context.PronunciationProgresses
                .Include(p => p.User)
                .Where(p => p.FlashCardId == flashCardId)
                .OrderByDescending(p => p.BestScore)
                .ToListAsync();
        }

        public async Task<List<PronunciationProgress>> GetByModuleIdAsync(int userId, int moduleId)
        {
            return await _context.PronunciationProgresses
                .Include(p => p.FlashCard)
                .Where(p => p.UserId == userId && p.FlashCard!.ModuleId == moduleId)
                .OrderBy(p => p.FlashCard!.FlashCardId)
                .ToListAsync();
        }

        public async Task<PronunciationProgress> CreateAsync(PronunciationProgress progress)
        {
            _context.PronunciationProgresses.Add(progress);
            await _context.SaveChangesAsync();
            return progress;
        }

        public async Task<PronunciationProgress> UpdateAsync(PronunciationProgress progress)
        {
            _context.PronunciationProgresses.Update(progress);
            await _context.SaveChangesAsync();
            return progress;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var progress = await _context.PronunciationProgresses.FindAsync(id);
            if (progress == null) return false;

            _context.PronunciationProgresses.Remove(progress);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PronunciationProgress> UpsertAsync(
            int userId,
            int flashCardId,
            double accuracyScore,
            double fluencyScore,
            double completenessScore,
            double pronunciationScore,
            List<string> problemPhonemes,
            List<string> strongPhonemes,
            DateTime attemptTime)
        {
            var existing = await GetByUserAndFlashCardAsync(userId, flashCardId);

            if (existing == null)
            {
                // Create new progress
                existing = new PronunciationProgress
                {
                    UserId = userId,
                    FlashCardId = flashCardId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.PronunciationProgresses.Add(existing);
            }

            // Update progress with new realtime assessment data
            existing.UpdateAfterAssessment(
                accuracyScore,
                fluencyScore,
                completenessScore,
                pronunciationScore,
                problemPhonemes,
                strongPhonemes,
                attemptTime
            );

            await _context.SaveChangesAsync();
            return existing;
        }

        // Statistics methods
        public async Task<int> GetTotalProgressCountAsync(int userId)
        {
            return await _context.PronunciationProgresses
                .Where(p => p.UserId == userId && p.TotalAttempts > 0)
                .CountAsync();
        }

        public async Task<int> GetCompletedCountAsync(int userId)
        {
            return await _context.PronunciationProgresses
                .Where(p => p.UserId == userId && p.BestScore >= 80)
                .CountAsync();
        }

        public async Task<int> GetNeedsReviewCountAsync(int userId)
        {
            return await _context.PronunciationProgresses
                .Where(p => p.UserId == userId && p.BestScore < 70)
                .CountAsync();
        }

        public async Task<double> GetAverageScoreAsync(int userId)
        {
            var progresses = await _context.PronunciationProgresses
                .Where(p => p.UserId == userId && p.TotalAttempts > 0)
                .ToListAsync();

            return progresses.Count != 0 ? progresses.Average(p => p.BestScore) : 0;
        }
    }
}
