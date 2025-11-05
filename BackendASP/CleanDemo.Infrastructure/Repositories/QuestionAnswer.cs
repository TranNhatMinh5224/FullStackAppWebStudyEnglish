using CleanDemo.Application.Interface;
using CleanDemo.Domain.Entities;
using CleanDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace CleanDemo.Infrastructure.Repositories
{
    public class QuestionAnswerRepository :  IQuestionAnswerRepository{
        private readonly AppDbContext _context;
         public QuestionAnswerRepository(AppDbContext context)
        {
            _context = context;
        }
        public Task<>ß

    } 
}