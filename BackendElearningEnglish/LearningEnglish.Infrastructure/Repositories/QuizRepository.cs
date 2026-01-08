using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class QuizRepository : IQuizRepository
    {
        private readonly AppDbContext _context;

        public QuizRepository(AppDbContext context)
        {
            _context = context;
        }
        // Thêm mới Quiz    

        public async Task AddQuizAsync(Quiz quiz)
        {
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();
        }
        // Cập nhật Quiz


        public async Task UpdateQuizAsync(Quiz quiz)
        {
            _context.Quizzes.Update(quiz);
            await _context.SaveChangesAsync();
        }
        // Xóa Quiz

        public async Task DeleteQuizAsync(int quizId)
        {
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz != null)
            {
                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();
            }
        }
        // Lấy thông tin  Quiz theo ID

        public async Task<Quiz?> GetQuizByIdAsync(int quizId)
        {
            return await _context.Quizzes.FindAsync(quizId);
        }
        // Lấy danh sách Quiz theo Assessment ID

        public async Task<List<Quiz>> GetQuizzesByAssessmentIdAsync(int assessmentId)
        {
            return await _context.Quizzes
                .Where(q => q.AssessmentId == assessmentId)
                .ToListAsync();
        }


        // Kiểm tra tồn tại Sections và Groups
        public async Task<bool> HasSectionsAsync(int quizId)
        {
            return await _context.QuizSections.AnyAsync(qs => qs.QuizId == quizId);
        }
        // Kiểm tra tồn tại Groups trong Quiz

        public async Task<bool> HasGroupsAsync(int quizId)
        {

            return await _context.QuizGroups
                .AnyAsync(qg => _context.QuizSections
                    .Any(qs => qs.QuizSectionId == qg.QuizSectionId && qs.QuizId == quizId));
        }
        // Lấy đầy đủ cấu trúc Quiz với Sections, Groups, Questions và Options
        public async Task<Quiz?> GetFullQuizAsync(int quizId)
        {
            return await _context.Quizzes
                .AsNoTracking()
                .Where(q => q.QuizId == quizId)
                .Include(q => q.QuizSections)
                    .ThenInclude(s => s.QuizGroups)
                        .ThenInclude(g => g.Questions)
                            .ThenInclude(qn => qn.Options)
                .Include(q => q.QuizSections)
                    .ThenInclude(s => s.Questions) // Load standalone questions
                        .ThenInclude(qn => qn.Options)
                .FirstOrDefaultAsync();
        }
    }
}