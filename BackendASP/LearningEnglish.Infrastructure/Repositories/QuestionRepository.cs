using Microsoft.EntityFrameworkCore;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly AppDbContext _context;

        public QuestionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddQuestionAsync(Question question)
        {
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
        }

        public async Task AddBulkQuestionsAsync(List<Question> questions)
        {
            if (questions == null || questions.Count == 0)
                return;

            
            await _context.Questions.AddRangeAsync(questions);
            await _context.SaveChangesAsync();

           
        }

        public async Task<List<int>> AddBulkQuestionsWithTransactionAsync(List<Question> questions)
        {
            if (questions == null || questions.Count == 0)
                return new List<int>();

            var createdQuestionIds = new List<int>();

            using var transaction = await _context.Database.BeginTransactionAsync();

            await _context.Questions.AddRangeAsync(questions);
            await _context.SaveChangesAsync();

            createdQuestionIds = questions.Select(q => q.QuestionId).ToList();

            await transaction.CommitAsync();

            return createdQuestionIds;
        }

        public async Task<Question?> GetQuestionByIdAsync(int questionId)
        {
            return await _context.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.QuestionId == questionId);
        }

        public async Task<List<Question>> GetQuestionsByQuizGroupIdAsync(int quizGroupId)
        {
            return await _context.Questions
                .Include(q => q.Options)
                .Where(q => q.QuizGroupId == quizGroupId)
                .ToListAsync();
        }

        public async Task<List<Question>> GetQuestionsByQuizSectionIdAsync(int quizSectionId)
        {
            return await _context.Questions
                .Include(q => q.Options)
                .Where(q => q.QuizSectionId == quizSectionId)
                .ToListAsync();
        }

        public async Task UpdateQuestionAsync(Question question)
        {
            question.UpdatedAt = DateTime.UtcNow;
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteQuestionAsync(int questionId)
        {
            var question = await _context.Questions.FindAsync(questionId);
            if (question != null)
            {
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
            }
        }
    }
}
