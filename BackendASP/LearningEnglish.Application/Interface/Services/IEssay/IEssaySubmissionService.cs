using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IEssaySubmissionService
    {
        // Nộp bài essay
        Task<ServiceResponse<EssaySubmissionDto>> CreateSubmissionAsync(CreateEssaySubmissionDto dto, int userId);
        
        // Lấy bài nộp phân trang
        Task<ServiceResponse<PagedResult<EssaySubmissionListDto>>> GetSubmissionsByEssayIdPagedAsync(int essayId, PageRequest request);
        
        // Lấy danh sách bài nộp
        Task<ServiceResponse<List<EssaySubmissionListDto>>> GetSubmissionsByEssayIdAsync(int essayId);
        
        // Lấy chi tiết bài nộp
        Task<ServiceResponse<EssaySubmissionDto>> GetSubmissionByIdAsync(int submissionId, int? userId = null);
        
        // Kiểm tra đã nộp chưa
        Task<ServiceResponse<EssaySubmissionDto?>> GetUserSubmissionForEssayAsync(int userId, int essayId);
        
        // Cập nhật bài nộp
        Task<ServiceResponse<EssaySubmissionDto>> UpdateSubmissionAsync(int submissionId, UpdateEssaySubmissionDto dto, int userId);
        
        // Xóa bài nộp
        Task<ServiceResponse<bool>> DeleteSubmissionAsync(int submissionId, int userId);
        
        // Tải file đính kèm
        Task<ServiceResponse<(Stream fileStream, string fileName, string contentType)>> DownloadSubmissionFileAsync(int submissionId);
    }
}