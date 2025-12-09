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

            // EF Core sẽ tự động insert cả Options vì có navigation property
            // và Options được thêm vào Questions.Options collection
            await _context.Questions.AddRangeAsync(questions);
            await _context.SaveChangesAsync();

            // Sau khi SaveChanges, QuestionId và AnswerOptionId sẽ được auto-generate
        }

        public async Task<List<int>> AddBulkQuestionsWithTransactionAsync(List<Question> questions)
        {
            if (questions == null || questions.Count == 0)
                return new List<int>();

            var createdQuestionIds = new List<int>();

            // Sử dụng transaction để đảm bảo tất cả câu hỏi được insert hoặc rollback nếu có lỗi
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Batch insert tất cả câu hỏi và options
                await _context.Questions.AddRangeAsync(questions);
                await _context.SaveChangesAsync();

                // Lấy danh sách ID đã tạo
                createdQuestionIds = questions.Select(q => q.QuestionId).ToList();

                // Commit transaction
                await transaction.CommitAsync();

                return createdQuestionIds;
            }
            catch (Exception)
            {
                // Rollback nếu có lỗi
                await transaction.RollbackAsync();
                throw;
            }
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
