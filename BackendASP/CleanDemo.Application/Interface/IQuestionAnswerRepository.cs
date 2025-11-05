using CleanDemo.Application.DTOs;
using CleanDemo.Application.Common;

namespace CleanDemo.Application.Interface{
    public interface IQuestionAnswerRepository { 
    Task CreateQuestionAsync(CreateQuestionDto questionDto);
    Task<List<QuestionDto>> ListQuestionAnswerAsync();
    }
}