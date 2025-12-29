using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class ModuleRepository : IModuleRepository
    {
        private readonly AppDbContext _context;

        public ModuleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Module?> GetByIdAsync(int moduleId)
        {
            return await _context.Modules
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ModuleId == moduleId);
        }

        public async Task<Module?> GetByIdForTeacherAsync(int moduleId, int teacherId)
        {
            return await _context.Modules
                .AsNoTracking()
                .Join(_context.Lessons,
                    m => m.LessonId,
                    l => l.LessonId,
                    (m, l) => new { Module = m, Lesson = l })
                .Join(_context.Courses,
                    ml => ml.Lesson.CourseId,
                    c => c.CourseId,
                    (ml, c) => new { ml.Module, ml.Lesson, Course = c })
                .Where(x => x.Module.ModuleId == moduleId && x.Course.TeacherId == teacherId)
                .Select(x => x.Module)
                .FirstOrDefaultAsync();
        }

        public async Task<Module?> GetByIdWithDetailsAsync(int moduleId)
        {
            return await _context.Modules
                .AsNoTracking()
                .Include(m => m.Lesson)
                .Include(m => m.Lectures)
                .Include(m => m.FlashCards)
                .Include(m => m.Assessments)
                .FirstOrDefaultAsync(m => m.ModuleId == moduleId);
        }

        public async Task<List<Module>> GetByLessonIdAsync(int lessonId)
        {
            return await _context.Modules
                .AsNoTracking()
                .Where(m => m.LessonId == lessonId)
                .OrderBy(m => m.OrderIndex)
                .ToListAsync();
        }

        public async Task<List<Module>> GetByLessonIdWithDetailsAsync(int lessonId)
        {
            return await _context.Modules
                .AsNoTracking()
                .Include(m => m.Lesson)
                .Include(m => m.Lectures)
                .Include(m => m.FlashCards)
                .Include(m => m.Assessments)
                .Where(m => m.LessonId == lessonId)
                .OrderBy(m => m.OrderIndex)
                .ToListAsync();
        }

        public async Task<List<Module>> GetByLessonIdForTeacherAsync(int lessonId, int teacherId)
        {
            return await _context.Modules
                .AsNoTracking()
                .Join(_context.Lessons,
                    m => m.LessonId,
                    l => l.LessonId,
                    (m, l) => new { Module = m, Lesson = l })
                .Join(_context.Courses,
                    ml => ml.Lesson.CourseId,
                    c => c.CourseId,
                    (ml, c) => new { ml.Module, ml.Lesson, Course = c })
                .Where(x => x.Module.LessonId == lessonId && x.Course.TeacherId == teacherId)
                .Select(x => x.Module)
                .OrderBy(m => m.OrderIndex)
                .ToListAsync();
        }

        public async Task<Module> CreateAsync(Module module)
        {
            _context.Modules.Add(module);
            await _context.SaveChangesAsync();
            return module;
        }

        public async Task<Module> UpdateAsync(Module module)
        {
            module.UpdatedAt = DateTime.UtcNow;
            _context.Modules.Update(module);
            await _context.SaveChangesAsync();
            return module;
        }

        public async Task<bool> DeleteAsync(int moduleId)
        {
            var module = await _context.Modules.FindAsync(moduleId);
            if (module == null) return false;

            _context.Modules.Remove(module);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int moduleId)
        {
            return await _context.Modules
                .AnyAsync(m => m.ModuleId == moduleId);
        }

        public async Task<int> GetMaxOrderIndexAsync(int lessonId)
        {
            var maxOrder = await _context.Modules
                .Where(m => m.LessonId == lessonId)
                .MaxAsync(m => (int?)m.OrderIndex);

            return maxOrder ?? 0;
        }

        public async Task<int?> GetLessonIdByModuleIdAsync(int moduleId)
        {
            return await _context.Modules
                .Where(m => m.ModuleId == moduleId)
                .Select(m => m.LessonId)
                .FirstOrDefaultAsync();
        }

        public async Task<Module?> GetModuleWithCourseAsync(int moduleId)
        {
            return await _context.Modules
                .AsNoTracking()
                .Include(m => m.Lesson)
                .ThenInclude(l => l!.Course)
                .FirstOrDefaultAsync(m => m.ModuleId == moduleId);
        }

        public async Task<Module?> GetModuleWithCourseForTeacherAsync(int moduleId, int teacherId)
        {
            return await _context.Modules
                .AsNoTracking()
                .Include(m => m.Lesson)
                .ThenInclude(l => l!.Course)
                .Where(m => m.ModuleId == moduleId && m.Lesson!.Course!.TeacherId == teacherId)
                .FirstOrDefaultAsync();
        }
    }
}
