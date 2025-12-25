using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface.Services;

public interface IEssayGradingService
{
    Task<ServiceResponse<EssayGradingResultDto>> GradeEssayWithAIAsync(int submissionId, CancellationToken cancellationToken = default);
    Task<ServiceResponse<EssayGradingResultDto>> GradeByTeacherAsync(int submissionId, TeacherGradingDto dto, int teacherId, CancellationToken cancellationToken = default);
}
