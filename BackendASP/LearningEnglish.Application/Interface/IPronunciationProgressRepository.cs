using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IPronunciationProgressRepository
    {
        // Basic CRUD
        Task<PronunciationProgress?> GetByIdAsync(int id);
        Task<PronunciationProgress?> GetByUserAndFlashCardAsync(int userId, int flashCardId);
        Task<List<PronunciationProgress>> GetByUserIdAsync(int userId);
        Task<List<PronunciationProgress>> GetByFlashCardIdAsync(int flashCardId);
        Task<List<PronunciationProgress>> GetByModuleIdAsync(int userId, int moduleId);
        Task<PronunciationProgress> CreateAsync(PronunciationProgress progress);
        Task<PronunciationProgress> UpdateAsync(PronunciationProgress progress);
        Task<bool> DeleteAsync(int id);

        // Upsert (Create or Update) - No longer depends on PronunciationAssessment entity
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
