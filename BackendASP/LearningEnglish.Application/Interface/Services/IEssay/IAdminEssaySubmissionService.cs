using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IAdminEssaySubmissionService
    {
        // Lấy danh sách submissions theo essay ID với phân trang
        Task<ServiceResponse<PagedResult<EssaySubmissionListDto>>> GetSubmissionsByEssayIdPagedAsync(int essayId, PageRequest request);
        
        // Lấy tất cả submissions của một essay
        Task<ServiceResponse<List<EssaySubmissionListDto>>> GetSubmissionsByEssayIdAsync(int essayId);
        
        // Lấy chi tiết submission bất kỳ (không check ownership)
        Task<ServiceResponse<EssaySubmissionDto>> GetSubmissionDetailAsync(int submissionId);
        
        // Xóa submission bất kỳ (hard delete)
        Task<ServiceResponse<bool>> DeleteSubmissionAsync(int submissionId);
        
        // Tải file đính kèm của submission
        Task<ServiceResponse<(Stream fileStream, string fileName, string contentType)>> DownloadSubmissionFileAsync(int submissionId);
    }
}
