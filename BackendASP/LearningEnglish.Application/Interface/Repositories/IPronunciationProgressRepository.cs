using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IPronunciationProgressRepository
    {
        // Lấy tiến độ theo ID
        Task<PronunciationProgress?> GetByIdAsync(int id);
        
        // Lấy tiến độ của user cho flashcard
        Task<PronunciationProgress?> GetByUserAndFlashCardAsync(int userId, int flashCardId);
        
        // Lấy tiến độ của user
        Task<List<PronunciationProgress>> GetByUserIdAsync(int userId);
        
        // Lấy tiến độ theo flashcard
        Task<List<PronunciationProgress>> GetByFlashCardIdAsync(int flashCardId);
        
        // Lấy tiến độ theo module
        Task<List<PronunciationProgress>> GetByModuleIdAsync(int userId, int moduleId);
        
        // Tạo tiến độ
        Task<PronunciationProgress> CreateAsync(PronunciationProgress progress);
        
        // Cập nhật tiến độ
        Task<PronunciationProgress> UpdateAsync(PronunciationProgress progress);
        
        // Xóa tiến độ
        Task<bool> DeleteAsync(int id);

        // Tạo hoặc cập nhật tiến độ
        Task<PronunciationProgress> UpsertAsync(
            int userId,
            int flashCardId,
            double accuracyScore,
            double fluencyScore,
            double completenessScore,
            double pronunciationScore,
            List<string> problemPhonemes,
            List<string> strongPhonemes,
            DateTime attemptTime);
    }
}
