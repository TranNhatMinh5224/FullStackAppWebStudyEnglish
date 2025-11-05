using CleanDemo.Application.Interface;
using CleanDemo.Domain.Entities;
using CleanDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace CleanDemo.Infrastructure.Repositories
{
    public class MiniTestRepository : IMiniTestRepository
    {
        private readonly AppDbContext _context;

        public MiniTestRepository(AppDbContext context)
        {
            _context = context;
        }
        // Thêm MiniTest
        public async Task AddMiniTestAsync(MiniTest miniTest)
        {
            await _context.MiniTests.AddAsync(miniTest);
            await _context.SaveChangesAsync();
        }
        // Kiểm tra MiniTest có tồn tại trong Lesson
        public async Task<bool> MiniTestExistsInLesson(string title, int lessonId)
        {
            return await _context.MiniTests.AnyAsync(mt => mt.Title == title && mt.LessonId == lessonId);
        }
        // Lấy danh sách MiniTest theo LessonId
        public async Task<List<MiniTest>?> GetListMiniTestByIdLesson(int lessonId)
        {
            return await _context.MiniTests
                .Where(mt => mt.LessonId == lessonId)
                .ToListAsync();
        }
        // Lấy MiniTest theo Id
        public async Task<MiniTest?> GetMiniTestByIdAsync(int miniTestId)
        {
            return await _context.MiniTests
                .Include(mt => mt.Lesson)
                .ThenInclude(l => l!.Course)
                .FirstOrDefaultAsync(mt => mt.MiniTestId == miniTestId);
        }
        // Cập nhật MiniTest
        public async Task UpdateMiniTestAsync(MiniTest miniTest)
        {
            _context.MiniTests.Update(miniTest);
            await _context.SaveChangesAsync();
        }
    
    }
}