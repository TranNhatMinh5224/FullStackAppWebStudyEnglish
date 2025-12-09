using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IEssaySubmissionRepository
    {
        // CRUD cho EssaySubmission (bài nộp của học sinh)
        Task<EssaySubmission> CreateSubmissionAsync(EssaySubmission submission);
        Task<EssaySubmission?> GetSubmissionByIdAsync(int submissionId);
        Task<EssaySubmission?> GetSubmissionByIdWithDetailsAsync(int submissionId);
        Task<List<EssaySubmission>> GetSubmissionsByEssayIdAsync(int essayId);
        Task<List<EssaySubmission>> GetSubmissionsByUserIdAsync(int userId);
        Task<List<EssaySubmission>> GetSubmissionsByAssessmentIdAsync(int assessmentId);
        Task<EssaySubmission?> GetUserSubmissionForEssayAsync(int userId, int essayId);
        Task<EssaySubmission> UpdateSubmissionAsync(EssaySubmission submission);
        Task DeleteSubmissionAsync(int submissionId);

        // Kiểm tra quyền hạn và tồn tại
        Task<bool> IsUserOwnerOfSubmissionAsync(int userId, int submissionId);
        Task<bool> AssessmentExistsAsync(int assessmentId);
    }
}