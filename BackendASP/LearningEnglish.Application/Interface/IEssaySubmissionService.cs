using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IEssaySubmissionService
    {
        // Student - Submit essay
        Task<ServiceResponse<EssaySubmissionDto>> CreateSubmissionAsync(CreateEssaySubmissionDto dto, int userId);
        
        // Teacher - Get submissions list by essay (WITH pagination) - Returns EssaySubmissionListDto
        Task<ServiceResponse<PagedResult<EssaySubmissionListDto>>> GetSubmissionsByEssayIdPagedAsync(int essayId, PageRequest request);
        
        // Teacher - Get submissions list by essay (WITHOUT pagination) - Returns EssaySubmissionListDto
        Task<ServiceResponse<List<EssaySubmissionListDto>>> GetSubmissionsByEssayIdAsync(int essayId);
        
        // Teacher/Student - Get submission detail by ID (với RLS policy)
        Task<ServiceResponse<EssaySubmissionDto>> GetSubmissionByIdAsync(int submissionId);
        
        // Student - Check if submitted
        Task<ServiceResponse<EssaySubmissionDto?>> GetUserSubmissionForEssayAsync(int userId, int essayId);
        
        // Student - Update own submission
        Task<ServiceResponse<EssaySubmissionDto>> UpdateSubmissionAsync(int submissionId, UpdateEssaySubmissionDto dto, int userId);
        
        // Student - Delete own submission
        Task<ServiceResponse<bool>> DeleteSubmissionAsync(int submissionId, int userId);
        
        // Teacher - Download file attachment of submission
        Task<ServiceResponse<(Stream fileStream, string fileName, string contentType)>> DownloadSubmissionFileAsync(int submissionId);
    }
}