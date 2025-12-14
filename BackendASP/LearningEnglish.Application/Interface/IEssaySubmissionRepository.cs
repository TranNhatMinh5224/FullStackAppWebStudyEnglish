using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IEssaySubmissionRepository
    {
        // CRUD cho EssaySubmission (bài nộp của học sinh)
        Task<EssaySubmission> CreateSubmissionAsync(EssaySubmission submission);
        Task<EssaySubmission?> GetSubmissionByIdAsync(int submissionId);
        Task<List<EssaySubmission>> GetSubmissionsByEssayIdPagedAsync(int essayId, int pageNumber, int pageSize);
        Task<List<EssaySubmission>> GetSubmissionsByEssayIdAsync(int essayId); // Non-paginated version
        Task<int> GetSubmissionsCountByEssayIdAsync(int essayId);
        Task<EssaySubmission?> GetUserSubmissionForEssayAsync(int userId, int essayId);
        Task<EssaySubmission> UpdateSubmissionAsync(EssaySubmission submission);
        Task DeleteSubmissionAsync(int submissionId);

        // Kiểm tra quyền hạn và tồn tại
        Task<bool> IsUserOwnerOfSubmissionAsync(int userId, int submissionId);
    }
}