using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IEssayService
    {
        // CRUD cho Essay (Bài kiểm tra tự luận)
        Task<ServiceResponse<EssayDto>> CreateEssayAsync(CreateEssayDto dto, int? teacherId = null);
        Task<ServiceResponse<EssayDto>> GetEssayByIdAsync(int essayId);
        Task<ServiceResponse<List<EssayDto>>> GetEssaysByAssessmentIdAsync(int assessmentId);
        Task<ServiceResponse<EssayDto>> UpdateEssayAsync(int essayId, UpdateEssayDto dto, int? teacherId = null);
        Task<ServiceResponse<bool>> DeleteEssayAsync(int essayId, int? teacherId = null);
    }
}