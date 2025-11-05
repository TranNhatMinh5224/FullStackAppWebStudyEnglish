using System.Collections.Generic;
using System.Threading.Tasks;
using CleanDemo.Domain.Entities;

namespace CleanDemo.Application.Interface
{
    public interface IQuestionAnswerRepository
    {
        Task<Question> AddQuestionWithAnswersAsync(Question question, List<AnswerOption> answers);
    }
}