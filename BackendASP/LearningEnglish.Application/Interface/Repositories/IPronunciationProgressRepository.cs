using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IPronunciationProgressRepository
    {
        // Lấy tiến độ theo ID
        // RLS: User chỉ xem progress của chính mình, Admin xem tất cả (có permission)
        Task<PronunciationProgress?> GetByIdAsync(int id);
        
        // Lấy tiến độ của user cho flashcard
        // RLS: User chỉ xem progress của chính mình, Admin xem tất cả (có permission)
        // userId parameter: Defense in depth (RLS + userId filter)
        Task<PronunciationProgress?> GetByUserAndFlashCardAsync(int userId, int flashCardId);
        
        // Lấy tiến độ của user
        // RLS: User chỉ xem progress của chính mình, Admin xem tất cả (có permission)
        // userId parameter: Defense in depth (RLS + userId filter) + Admin có thể query progress của user khác
        Task<List<PronunciationProgress>> GetByUserIdAsync(int userId);
        
        // Lấy tiến độ theo flashcard (public - leaderboard)
        // RLS: Không filter theo user (public data)
        Task<List<PronunciationProgress>> GetByFlashCardIdAsync(int flashCardId);
        
        // Lấy tiến độ theo module
        // RLS: User chỉ xem progress của chính mình, Admin xem tất cả (có permission)
        // userId parameter: Defense in depth (RLS + userId filter)
        Task<List<PronunciationProgress>> GetByModuleIdAsync(int userId, int moduleId);
        
        // Tạo tiến độ
        Task<PronunciationProgress> CreateAsync(PronunciationProgress progress);
        
        // Cập nhật tiến độ
        Task<PronunciationProgress> UpdateAsync(PronunciationProgress progress);
        
        // Xóa tiến độ
        Task<bool> DeleteAsync(int id);

        // Tạo hoặc cập nhật tiến độ
        // RLS: User chỉ tạo/update progress của chính mình
        // userId parameter: Defense in depth (RLS + userId filter)
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
