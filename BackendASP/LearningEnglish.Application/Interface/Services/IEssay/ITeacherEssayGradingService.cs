using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface ITeacherEssayGradingService
    {
        // Teacher grade với AI (validate essay ownership)
        Task<ServiceResponse<EssayGradingResultDto>> GradeEssayWithAIAsync(int submissionId, int teacherId, CancellationToken cancellationToken = default);
        
        // Teacher grade thủ công (validate essay ownership)
        Task<ServiceResponse<EssayGradingResultDto>> GradeEssayAsync(int submissionId, TeacherGradingDto dto, int teacherId, CancellationToken cancellationToken = default);
    }
}
