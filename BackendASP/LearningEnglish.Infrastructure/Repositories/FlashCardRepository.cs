using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class FlashCardRepository : IFlashCardRepository
    {
        private readonly AppDbContext _context;

        public FlashCardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<FlashCard?> GetByIdAsync(int flashCardId)
        {
            return await _context.FlashCards
                .FirstOrDefaultAsync(fc => fc.FlashCardId == flashCardId);
        }

        public async Task<FlashCard?> GetByIdWithDetailsAsync(int flashCardId)
        {
            return await _context.FlashCards
                .Include(fc => fc.Module)
                    .ThenInclude(m => m!.Lesson)
                        .ThenInclude(l => l!.Course)
                .Include(fc => fc.Reviews)
                .FirstOrDefaultAsync(fc => fc.FlashCardId == flashCardId);
        }

        public async Task<List<FlashCard>> GetByModuleIdAsync(int moduleId)
        {
            return await _context.FlashCards
                .Where(fc => fc.ModuleId == moduleId)
                .OrderBy(fc => fc.Word)
                .ToListAsync();
        }

        public async Task<List<FlashCard>> GetByModuleIdWithDetailsAsync(int moduleId)
        {
            return await _context.FlashCards
                .Include(fc => fc.Module)
                .Include(fc => fc.Reviews)
                .Where(fc => fc.ModuleId == moduleId)
                .OrderBy(fc => fc.Word)
                .ToListAsync();
        }


        public async Task<int> GetFlashCardCountByModuleAsync(int moduleId)
        {
            return await _context.FlashCards
                .Where(fc => fc.ModuleId == moduleId)
                .CountAsync();
        }

        public async Task<FlashCard> CreateAsync(FlashCard flashCard)
        {
            flashCard.CreatedAt = DateTime.UtcNow;
            flashCard.UpdatedAt = DateTime.UtcNow;

            _context.FlashCards.Add(flashCard);
            await _context.SaveChangesAsync();

            return flashCard;
        }

        public async Task<FlashCard> UpdateAsync(FlashCard flashCard)
        {
            flashCard.UpdatedAt = DateTime.UtcNow;
            _context.FlashCards.Update(flashCard);
            await _context.SaveChangesAsync();

            return flashCard;
        }

        public async Task<bool> DeleteAsync(int flashCardId)
        {
            var flashCard = await GetByIdAsync(flashCardId);
            if (flashCard == null) return false;

            _context.FlashCards.Remove(flashCard);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int flashCardId)
        {
            return await _context.FlashCards.AnyAsync(fc => fc.FlashCardId == flashCardId);
        }

        public async Task<List<FlashCard>> CreateBulkAsync(List<FlashCard> flashCards)
        {
            foreach (var flashCard in flashCards)
            {
                flashCard.CreatedAt = DateTime.UtcNow;
                flashCard.UpdatedAt = DateTime.UtcNow;
            }

            _context.FlashCards.AddRange(flashCards);
            await _context.SaveChangesAsync();

            return flashCards;
        }



        public async Task<FlashCard?> GetFlashCardWithModuleCourseAsync(int flashCardId)
        {
            return await _context.FlashCards
                .Include(fc => fc.Module)
                    .ThenInclude(m => m!.Lesson)
                        .ThenInclude(l => l!.Course)
                .FirstOrDefaultAsync(fc => fc.FlashCardId == flashCardId);
        }
    }
}
