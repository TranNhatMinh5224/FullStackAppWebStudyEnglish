using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IPronunciationAssessmentRepository
    {
        Task<PronunciationAssessment> CreateAsync(PronunciationAssessment assessment);
        Task<PronunciationAssessment?> GetByIdAsync(int id);
        Task<List<PronunciationAssessment>> GetByUserIdAsync(int userId);
        Task<List<PronunciationAssessment>> GetByFlashCardIdAsync(int flashCardId);
        Task<List<PronunciationAssessment>> GetByAssignmentIdAsync(int assignmentId);
        Task UpdateAsync(PronunciationAssessment assessment);
        Task DeleteAsync(int id);
        Task<int> CountByUserIdAsync(int userId);
        
        // 🆕 Progress tracking methods
        Task<List<PronunciationAssessment>> GetByUserIdSinceDateAsync(int userId, DateTime sinceDate);
        Task<List<double>> GetAllUserAverageScoresAsync();
    }
}
