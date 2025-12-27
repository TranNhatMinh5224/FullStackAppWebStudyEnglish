using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface IAdminEssayGradingService
    {
        // Admin có thể grade bất kỳ submission nào với AI
        Task<ServiceResponse<EssayGradingResultDto>> GradeEssayWithAIAsync(int submissionId, CancellationToken cancellationToken = default);
        
        // Admin có thể grade thủ công bất kỳ submission nào
        Task<ServiceResponse<EssayGradingResultDto>> GradeByAdminAsync(int submissionId, TeacherGradingDto dto, CancellationToken cancellationToken = default);
    }
}
