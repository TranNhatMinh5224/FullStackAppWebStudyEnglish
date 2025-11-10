using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class FlashCardRepository : IFlashCardRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FlashCardRepository> _logger;

        public FlashCardRepository(AppDbContext context, ILogger<FlashCardRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // + Lấy flashcard theo ID
        public async Task<FlashCard?> GetByIdAsync(int flashCardId)
        {
            try
            {
                return await _context.FlashCards
                    .FirstOrDefaultAsync(fc => fc.FlashCardId == flashCardId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy FlashCard với ID: {FlashCardId}", flashCardId);
                throw;
            }
        }

        // + Lấy flashcard với thông tin chi tiết
        public async Task<FlashCard?> GetByIdWithDetailsAsync(int flashCardId)
        {
            try
            {
                return await _context.FlashCards
                    .Include(fc => fc.Module)
                        .ThenInclude(m => m!.Lesson)
                            .ThenInclude(l => l!.Course)
                    .Include(fc => fc.Reviews)
                    .Include(fc => fc.PronunciationAssessments)
                    .FirstOrDefaultAsync(fc => fc.FlashCardId == flashCardId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy FlashCard chi tiết với ID: {FlashCardId}", flashCardId);
                throw;
            }
        }

        // + Lấy danh sách flashcard theo module
        public async Task<List<FlashCard>> GetByModuleIdAsync(int moduleId)
        {
            try
            {
                return await _context.FlashCards
                    .Where(fc => fc.ModuleId == moduleId)
                    .OrderBy(fc => fc.Word)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách FlashCard theo ModuleId: {ModuleId}", moduleId);
                throw;
            }
        }

        // + Lấy danh sách flashcard với thông tin chi tiết
        public async Task<List<FlashCard>> GetByModuleIdWithDetailsAsync(int moduleId)
        {
            try
            {
                return await _context.FlashCards
                    .Include(fc => fc.Module)
                    .Include(fc => fc.Reviews)
                    .Where(fc => fc.ModuleId == moduleId)
                    .OrderBy(fc => fc.Word)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách FlashCard chi tiết theo ModuleId: {ModuleId}", moduleId);
                throw;
            }
        }

        // + Tạo flashcard mới
        public async Task<FlashCard> CreateAsync(FlashCard flashCard)
        {
            try
            {
                flashCard.CreatedAt = DateTime.UtcNow;
                flashCard.UpdatedAt = DateTime.UtcNow;

                _context.FlashCards.Add(flashCard);
                await _context.SaveChangesAsync();

                return flashCard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo FlashCard mới: {Word}", flashCard.Word);
                throw;
            }
        }

        // + Cập nhật flashcard
        public async Task<FlashCard> UpdateAsync(FlashCard flashCard)
        {
            try
            {
                flashCard.UpdatedAt = DateTime.UtcNow;
                _context.FlashCards.Update(flashCard);
                await _context.SaveChangesAsync();

                return flashCard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật FlashCard với ID: {FlashCardId}", flashCard.FlashCardId);
                throw;
            }
        }

        // + Xóa flashcard
        public async Task<bool> DeleteAsync(int flashCardId)
        {
            try
            {
                var flashCard = await GetByIdAsync(flashCardId);
                if (flashCard == null) return false;

                _context.FlashCards.Remove(flashCard);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa FlashCard với ID: {FlashCardId}", flashCardId);
                throw;
            }
        }

        // + Kiểm tra flashcard có tồn tại
        public async Task<bool> ExistsAsync(int flashCardId)
        {
            try
            {
                return await _context.FlashCards.AnyAsync(fc => fc.FlashCardId == flashCardId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra tồn tại FlashCard với ID: {FlashCardId}", flashCardId);
                throw;
            }
        }

        // + Tìm kiếm flashcard
        public async Task<List<FlashCard>> SearchFlashCardsAsync(string searchTerm, int? moduleId = null)
        {
            try
            {
                var query = _context.FlashCards.AsQueryable();

                if (moduleId.HasValue)
                    query = query.Where(fc => fc.ModuleId == moduleId.Value);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var lowerSearchTerm = searchTerm.ToLower();
                    query = query.Where(fc => fc.Word.ToLower().Contains(lowerSearchTerm) ||
                                            fc.Meaning.ToLower().Contains(lowerSearchTerm));
                }

                return await query.OrderBy(fc => fc.Word).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm FlashCard: {SearchTerm}", searchTerm);
                throw;
            }
        }

        // + Lấy flashcard theo từ
        public async Task<List<FlashCard>> GetFlashCardsByWordAsync(string word, int? moduleId = null)
        {
            try
            {
                var query = _context.FlashCards.Where(fc => fc.Word.ToLower() == word.ToLower());

                if (moduleId.HasValue)
                    query = query.Where(fc => fc.ModuleId == moduleId.Value);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy FlashCard theo từ: {Word}", word);
                throw;
            }
        }

        // + Kiểm tra từ đã tồn tại trong module
        public async Task<bool> WordExistsInModuleAsync(string word, int moduleId, int? excludeFlashCardId = null)
        {
            try
            {
                var query = _context.FlashCards.Where(fc => 
                    fc.ModuleId == moduleId && 
                    fc.Word.ToLower() == word.ToLower());

                if (excludeFlashCardId.HasValue)
                    query = query.Where(fc => fc.FlashCardId != excludeFlashCardId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra từ tồn tại: {Word} trong Module: {ModuleId}", word, moduleId);
                throw;
            }
        }



        // + Tạo nhiều flashcard cùng lúc
        public async Task<List<FlashCard>> CreateBulkAsync(List<FlashCard> flashCards)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo bulk FlashCard: {Count} cards", flashCards.Count);
                throw;
            }
        }



        // + Lấy flashcard với module và course để kiểm tra quyền
        public async Task<FlashCard?> GetFlashCardWithModuleCourseAsync(int flashCardId)
        {
            try
            {
                return await _context.FlashCards
                    .Include(fc => fc.Module)
                        .ThenInclude(m => m!.Lesson)
                            .ThenInclude(l => l!.Course)
                    .FirstOrDefaultAsync(fc => fc.FlashCardId == flashCardId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy FlashCard với Module Course để kiểm tra quyền: {FlashCardId}", flashCardId);
                throw;
            }
        }
    }
}
