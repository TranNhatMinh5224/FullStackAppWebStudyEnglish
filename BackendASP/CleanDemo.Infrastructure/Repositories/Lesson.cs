using CleanDemo.Domain.Entities;
using CleanDemo.Domain.Enums;
using CleanDemo.Application.Interface;
using CleanDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace CleanDemo.Infrastructure.Repositories
{
    public class LessonRepository : ILessonRepository
    {
        private readonly AppDbContext _context;

        public LessonRepository(AppDbContext context)
        {
            _context = context;
        }

        // CRUD cơ bản 

        /// <summary>
        /// Lấy danh sách bài học của CourseId
        /// </summary>
        public async Task<Lesson> GetListLessonByCourseId(int CourseId)
        {
            return await _context.Lessons.FirstOrDefaultAsync(c => c.CourseId == CourseId);
        }
    }
}