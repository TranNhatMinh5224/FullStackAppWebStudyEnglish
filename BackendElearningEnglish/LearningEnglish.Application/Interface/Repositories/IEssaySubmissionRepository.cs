using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IEssaySubmissionRepository
    {
        // Tạo bài nộp
        Task<EssaySubmission> CreateSubmissionAsync(EssaySubmission submission);
        
        // Lấy bài nộp theo ID
        Task<EssaySubmission?> GetSubmissionByIdAsync(int submissionId);
        
        // Lấy bài nộp theo essay với phân trang
        Task<List<EssaySubmission>> GetSubmissionsByEssayIdPagedAsync(int essayId, int pageNumber, int pageSize);
        
        // Lấy tất cả bài nộp theo essay
        Task<List<EssaySubmission>> GetSubmissionsByEssayIdAsync(int essayId);
        
        // Đếm số bài nộp theo essay
        Task<int> GetSubmissionsCountByEssayIdAsync(int essayId);
        
        // Lấy bài nộp của user cho essay
        Task<EssaySubmission?> GetUserSubmissionForEssayAsync(int userId, int essayId);
        
        // Cập nhật bài nộp
        Task<EssaySubmission> UpdateSubmissionAsync(EssaySubmission submission);
        
        // Xóa bài nộp
        Task DeleteSubmissionAsync(int submissionId);

        // Kiểm tra user sở hữu bài nộp
        Task<bool> IsUserOwnerOfSubmissionAsync(int userId, int submissionId);
    }
}