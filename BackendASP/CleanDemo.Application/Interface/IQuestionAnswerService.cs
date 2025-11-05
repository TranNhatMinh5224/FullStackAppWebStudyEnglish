namespace CleanDemo.Application.Interface
{
    using System.Threading.Tasks;
    using CleanDemo.Application.Common;
    using CleanDemo.Application.DTOs;

    public interface IQuestionAnswerService
    {
        Task<ServiceResponse<QuestionDto>> CreateQuestionAsync(CreateQuestionDto dto, int? actorUserId = null);
    }
}
