using CleanDemo.Application.Interface;
using CleanDemo.Domain.Entities;
using CleanDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace CleanDemo.Infrastructure.Repositories
{
    public class QuestionAnswerRepository : IQuestionAnswerRepository
    {
        private readonly AppDbContext _context;
        public QuestionAnswerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Question> AddQuestionWithAnswersAsync(Question question, List<AnswerOption> answers)
        {
            await _context.Questions.AddAsync(question);
            await _context.SaveChangesAsync();

            foreach (var a in answers)
            {
                a.QuestionId = question.QuestionId;
            }

            await _context.AnswerOptions.AddRangeAsync(answers);
            await _context.SaveChangesAsync();

            await _context.Entry(question).Collection(q => q.Options).LoadAsync();
            return question;
        }
    }
}