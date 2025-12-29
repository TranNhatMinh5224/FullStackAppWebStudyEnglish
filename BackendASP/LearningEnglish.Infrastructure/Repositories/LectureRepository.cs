using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class LectureRepository : ILectureRepository
    {
        private readonly AppDbContext _context;

        public LectureRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Lecture?> GetByIdAsync(int lectureId)
        {
            return await _context.Lectures
                .FirstOrDefaultAsync(l => l.LectureId == lectureId);
        }

        public async Task<Lecture?> GetByIdForTeacherAsync(int lectureId, int teacherId)
        {
            return await _context.Lectures
                .Join(_context.Modules,
                    l => l.ModuleId,
                    m => m.ModuleId,
                    (l, m) => new { Lecture = l, Module = m })
                .Join(_context.Lessons,
                    lm => lm.Module.LessonId,
                    lesson => lesson.LessonId,
                    (lm, lesson) => new { lm.Lecture, lm.Module, Lesson = lesson })
                .Join(_context.Courses,
                    lml => lml.Lesson.CourseId,
                    c => c.CourseId,
                    (lml, c) => new { lml.Lecture, lml.Module, lml.Lesson, Course = c })
                .Where(x => x.Lecture.LectureId == lectureId && x.Course.TeacherId == teacherId)
                .Select(x => x.Lecture)
                .FirstOrDefaultAsync();
        }

        public async Task<Lecture?> GetByIdWithDetailsAsync(int lectureId)
        {
            return await _context.Lectures
                .Include(l => l.Module)
                    .ThenInclude(m => m!.Lesson)
                        .ThenInclude(lesson => lesson!.Course)
                .Include(l => l.Parent)
                .Include(l => l.Children)
                .FirstOrDefaultAsync(l => l.LectureId == lectureId);
        }

        public async Task<List<Lecture>> GetByModuleIdAsync(int moduleId)
        {
            return await _context.Lectures
                .Where(l => l.ModuleId == moduleId)
                .OrderBy(l => l.OrderIndex)
                .ThenBy(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Lecture>> GetByModuleIdWithDetailsAsync(int moduleId)
        {
            return await _context.Lectures
                .Include(l => l.Module)
                .Include(l => l.Parent)
                .Include(l => l.Children)
                .Where(l => l.ModuleId == moduleId)
                .OrderBy(l => l.OrderIndex)
                .ThenBy(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<Lecture> CreateAsync(Lecture lecture)
        {
            lecture.CreatedAt = DateTime.UtcNow;
            lecture.UpdatedAt = DateTime.UtcNow;

            _context.Lectures.Add(lecture);
            await _context.SaveChangesAsync();

            return lecture;
        }

        public async Task<Lecture> UpdateAsync(Lecture lecture)
        {
            lecture.UpdatedAt = DateTime.UtcNow;
            _context.Lectures.Update(lecture);
            await _context.SaveChangesAsync();

            return lecture;
        }

        public async Task<bool> DeleteAsync(int lectureId)
        {
            var lecture = await GetByIdAsync(lectureId);
            if (lecture == null) return false;

            _context.Lectures.Remove(lecture);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int lectureId)
        {
            return await _context.Lectures.AnyAsync(l => l.LectureId == lectureId);
        }

        public async Task<List<Lecture>> GetTreeByModuleIdAsync(int moduleId)
        {
            return await _context.Lectures
                .Include(l => l.Children.OrderBy(c => c.OrderIndex))
                .Where(l => l.ModuleId == moduleId)
                .OrderBy(l => l.OrderIndex)
                .ToListAsync();
        }

        public async Task<bool> HasChildrenAsync(int lectureId)
        {
            return await _context.Lectures.AnyAsync(l => l.ParentLectureId == lectureId);
        }

        public async Task<int> GetMaxOrderIndexAsync(int moduleId, int? parentLectureId = null)
        {
            var query = _context.Lectures.Where(l => l.ModuleId == moduleId);

            if (parentLectureId.HasValue)
                query = query.Where(l => l.ParentLectureId == parentLectureId.Value);
            else
                query = query.Where(l => l.ParentLectureId == null);

            var maxOrder = await query.MaxAsync(l => (int?)l.OrderIndex);
            return maxOrder ?? 0;
        }

        public async Task<Lecture?> GetLectureWithModuleCourseAsync(int lectureId)
        {
            return await _context.Lectures
                .Include(l => l.Module)
                    .ThenInclude(m => m!.Lesson)
                        .ThenInclude(lesson => lesson!.Course)
                .FirstOrDefaultAsync(l => l.LectureId == lectureId);
        }

        public async Task<Lecture?> GetLectureWithModuleCourseForTeacherAsync(int lectureId, int teacherId)
        {
            return await _context.Lectures
                .Include(l => l.Module)
                    .ThenInclude(m => m!.Lesson)
                        .ThenInclude(lesson => lesson!.Course)
                .Where(l => l.LectureId == lectureId && l.Module!.Lesson!.Course!.TeacherId == teacherId)
                .FirstOrDefaultAsync();
        }
    }
}
