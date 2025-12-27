using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface ITeacherEssaySubmissionService
    {
        // Lấy submissions của essays thuộc teacher với phân trang
        Task<ServiceResponse<PagedResult<EssaySubmissionListDto>>> GetSubmissionsByEssayIdPagedAsync(int essayId, int teacherId, PageRequest request);
        
        // Lấy tất cả submissions của essay thuộc teacher
        Task<ServiceResponse<List<EssaySubmissionListDto>>> GetSubmissionsByEssayIdAsync(int essayId, int teacherId);
        
        // Lấy chi tiết submission với validation essay ownership
        Task<ServiceResponse<EssaySubmissionDto>> GetSubmissionDetailAsync(int submissionId, int teacherId);
        
        // Tải file đính kèm với validation essay ownership
        Task<ServiceResponse<(Stream fileStream, string fileName, string contentType)>> DownloadSubmissionFileAsync(int submissionId, int teacherId);
    }
}
