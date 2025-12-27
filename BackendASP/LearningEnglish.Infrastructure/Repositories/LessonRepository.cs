using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Interface;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace LearningEnglish.Infrastructure.Repositories
{
    public class LessonRepository : ILessonRepository
    {
        private readonly AppDbContext _context;

        public LessonRepository(AppDbContext context)
        {
            _context = context;
        }




        // Lấy danh sách bài học của CourseId
        
        public async Task<List<Lesson>> GetListLessonByCourseId(int CourseId)
        {
            return await _context.Lessons
                .Where(c => c.CourseId == CourseId) 
                .ToListAsync();
        }

        // Lấy chi tiết 1 bài học
       
        public async Task<Lesson?> GetLessonById(int lessonId)
        {
            return await _context.Lessons.FindAsync(lessonId);
        }
        // Thêm bài học

        public async Task AddLesson(Lesson lesson)
        {
            await _context.Lessons.AddAsync(lesson);
            await _context.SaveChangesAsync();
        }
        // Cập nhật bài học

        public async Task UpdateLesson(Lesson lesson)
        {
            _context.Lessons.Update(lesson);
            await _context.SaveChangesAsync();
        }
        // Xóa bài học 

        public async Task DeleteLesson(int lessonId)
        {
            var lesson = await _context.Lessons.FindAsync(lessonId);
            if (lesson != null)
            {
                _context.Lessons.Remove(lesson);
                await _context.SaveChangesAsync();
            }
        }
        // Kiểm tra sự tồn tại của lesson trong Course chưa 
        public async Task<bool> LessonIncourse(string newtitle, int courseId)
        {
            return await _context.Lessons.AnyAsync(l => l.Title == newtitle && l.CourseId == courseId);
        }
        /// Đếm số lesson trong course
        public async Task<int> CountLessonInCourse(int courseId)
        {
            return await _context.Lessons.CountAsync(l => l.CourseId == courseId);
        }

        public async Task<int?> GetCourseIdByLessonIdAsync(int lessonId)
        {
            return await _context.Lessons
                .Where(l => l.LessonId == lessonId)
                .Select(l => l.CourseId)
                .FirstOrDefaultAsync();
        }
    }
}
