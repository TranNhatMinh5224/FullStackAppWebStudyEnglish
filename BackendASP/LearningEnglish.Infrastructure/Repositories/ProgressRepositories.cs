using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class CourseProgressRepository : ICourseProgressRepository
    {
        private readonly AppDbContext _context;

        public CourseProgressRepository(AppDbContext context)
        {
            _context = context;
        }

        // RLS đã filter: User chỉ xem progress của chính mình, Teacher xem progress của students trong own courses, Admin xem tất cả
        // Defense in depth: Vẫn filter theo userId để đảm bảo đúng khi query progress của user cụ thể
        public async Task<CourseProgress?> GetByUserAndCourseAsync(int userId, int courseId)
        {
            // RLS đã filter: User chỉ query được progress của chính mình
            // Teacher có thể query progress của students trong own courses
            // Filter theo userId và courseId để đảm bảo đúng (defense in depth)
            return await _context.CourseProgresses
                .Include(cp => cp.Course)
                    .ThenInclude(c => c.Lessons)
                .FirstOrDefaultAsync(cp => cp.UserId == userId && cp.CourseId == courseId);
        }

        // RLS đã filter: User chỉ xem progress của chính mình
        // Defense in depth: Vẫn filter theo userId để đảm bảo đúng
        public async Task<List<CourseProgress>> GetByUserIdAsync(int userId)
        {
            // RLS đã filter: User chỉ query được progress của chính mình
            // Filter theo userId để đảm bảo đúng (defense in depth)
            return await _context.CourseProgresses
                .Include(cp => cp.Course)
                .Where(cp => cp.UserId == userId)
                .ToListAsync();
        }

        public async Task<int> CountCompletedCoursesByUserAsync(int userId)
        {
            return await _context.CourseProgresses
                .Where(cp => cp.UserId == userId && cp.IsCompleted)
                .CountAsync();
        }

        public async Task AddAsync(CourseProgress courseProgress)
        {
            await _context.CourseProgresses.AddAsync(courseProgress);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CourseProgress courseProgress)
        {
            _context.CourseProgresses.Update(courseProgress);
            await _context.SaveChangesAsync();
        }
    }

    public class LessonCompletionRepository : ILessonCompletionRepository
    {
        private readonly AppDbContext _context;

        public LessonCompletionRepository(AppDbContext context)
        {
            _context = context;
        }

        // RLS đã filter: User chỉ xem completions của chính mình, Teacher xem completions của students trong own courses, Admin xem tất cả
        // Defense in depth: Vẫn filter theo userId để đảm bảo đúng
        public async Task<LessonCompletion?> GetByUserAndLessonAsync(int userId, int lessonId)
        {
            // RLS đã filter: User chỉ query được completions của chính mình
            // Teacher có thể query completions của students trong own courses
            // Filter theo userId và lessonId để đảm bảo đúng (defense in depth)
            return await _context.LessonCompletions
                .FirstOrDefaultAsync(lc => lc.UserId == userId && lc.LessonId == lessonId);
        }

        // RLS đã filter: User chỉ xem completions của chính mình
        // Defense in depth: Vẫn filter theo userId để đảm bảo đúng
        public async Task<List<LessonCompletion>> GetByUserIdAsync(int userId)
        {
            // RLS đã filter: User chỉ query được completions của chính mình
            // Filter theo userId để đảm bảo đúng (defense in depth)
            return await _context.LessonCompletions
                .Where(lc => lc.UserId == userId)
                .ToListAsync();
        }

        // RLS đã filter: User chỉ xem completions của chính mình
        // Defense in depth: Vẫn filter theo userId để đảm bảo đúng
        public async Task<List<LessonCompletion>> GetByUserAndLessonIdsAsync(int userId, List<int> lessonIds)
        {
            // RLS đã filter: User chỉ query được completions của chính mình
            // Filter theo userId và lessonIds để đảm bảo đúng (defense in depth)
            return await _context.LessonCompletions
                .Where(lc => lc.UserId == userId && lessonIds.Contains(lc.LessonId))
                .ToListAsync();
        }

        public async Task<int> CountCompletedLessonsByUserAsync(int userId)
        {
            return await _context.LessonCompletions
                .Where(lc => lc.UserId == userId && lc.IsCompleted)
                .CountAsync();
        }

        public async Task AddAsync(LessonCompletion lessonCompletion)
        {
            await _context.LessonCompletions.AddAsync(lessonCompletion);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LessonCompletion lessonCompletion)
        {
            _context.LessonCompletions.Update(lessonCompletion);
            await _context.SaveChangesAsync();
        }
    }

    public class ModuleCompletionRepository : IModuleCompletionRepository
    {
        private readonly AppDbContext _context;

        public ModuleCompletionRepository(AppDbContext context)
        {
            _context = context;
        }

        // RLS đã filter: User chỉ xem completions của chính mình, Teacher xem completions của students trong own courses, Admin xem tất cả
        // Defense in depth: Vẫn filter theo userId để đảm bảo đúng
        public async Task<ModuleCompletion?> GetByUserAndModuleAsync(int userId, int moduleId)
        {
            // RLS đã filter: User chỉ query được completions của chính mình
            // Teacher có thể query completions của students trong own courses
            // Filter theo userId và moduleId để đảm bảo đúng (defense in depth)
            return await _context.ModuleCompletions
                .FirstOrDefaultAsync(mc => mc.UserId == userId && mc.ModuleId == moduleId);
        }

        // RLS đã filter: User chỉ xem completions của chính mình
        // Defense in depth: Vẫn filter theo userId để đảm bảo đúng
        public async Task<List<ModuleCompletion>> GetByUserAndModuleIdsAsync(int userId, List<int> moduleIds)
        {
            // RLS đã filter: User chỉ query được completions của chính mình
            // Filter theo userId và moduleIds để đảm bảo đúng (defense in depth)
            return await _context.ModuleCompletions
                .Where(mc => mc.UserId == userId && moduleIds.Contains(mc.ModuleId))
                .ToListAsync();
        }

        public async Task<int> CountCompletedModulesByUserAsync(int userId)
        {
            return await _context.ModuleCompletions
                .Where(mc => mc.UserId == userId && mc.IsCompleted)
                .CountAsync();
        }

        public async Task AddAsync(ModuleCompletion moduleCompletion)
        {
            await _context.ModuleCompletions.AddAsync(moduleCompletion);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ModuleCompletion moduleCompletion)
        {
            _context.ModuleCompletions.Update(moduleCompletion);
            await _context.SaveChangesAsync();
        }
    }
}
