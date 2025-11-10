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

        public async Task<bool> ReorderModulesAsync(int lessonId, List<(int ModuleId, int NewOrderIndex)> reorderItems)
        {
            foreach (var (moduleId, newOrderIndex) in reorderItems)
            {
                var module = await _context.Modules
                    .FirstOrDefaultAsync(m => m.ModuleId == moduleId && m.LessonId == lessonId);
                
                if (module != null)
                {
                    module.OrderIndex = newOrderIndex;
                    module.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetMaxOrderIndexAsync(int lessonId)
        {
            var maxOrder = await _context.Modules
                .Where(m => m.LessonId == lessonId)
                .MaxAsync(m => (int?)m.OrderIndex);
            
            return maxOrder ?? 0;
        }

        public async Task<Module?> GetNextModuleAsync(int currentModuleId)
        {
            var currentModule = await _context.Modules
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ModuleId == currentModuleId);
            
            if (currentModule == null) return null;

            return await _context.Modules
                .AsNoTracking()
                .Where(m => m.LessonId == currentModule.LessonId && 
                           m.OrderIndex > currentModule.OrderIndex)
                .OrderBy(m => m.OrderIndex)
                .FirstOrDefaultAsync();
        }

        public async Task<Module?> GetPreviousModuleAsync(int currentModuleId)
        {
            var currentModule = await _context.Modules
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ModuleId == currentModuleId);
            
            if (currentModule == null) return null;

            return await _context.Modules
                .AsNoTracking()
                .Where(m => m.LessonId == currentModule.LessonId && 
                           m.OrderIndex < currentModule.OrderIndex)
                .OrderByDescending(m => m.OrderIndex)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteMultipleAsync(List<int> moduleIds)
        {
            var modules = await _context.Modules
                .Where(m => moduleIds.Contains(m.ModuleId))
                .ToListAsync();

            if (modules.Any())
            {
                _context.Modules.RemoveRange(modules);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<List<Module>> DuplicateModulesToLessonAsync(List<int> moduleIds, int targetLessonId)
        {
            var originalModules = await _context.Modules
                .AsNoTracking()
                .Where(m => moduleIds.Contains(m.ModuleId))
                .ToListAsync();

            var maxOrderIndex = await GetMaxOrderIndexAsync(targetLessonId);
            var duplicatedModules = new List<Module>();

            foreach (var original in originalModules)
            {
                var duplicate = new Module
                {
                    LessonId = targetLessonId,
                    Name = $"{original.Name} (Copy)",
                    Description = original.Description,
                    OrderIndex = ++maxOrderIndex,
                    ContentType = original.ContentType,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                duplicatedModules.Add(duplicate);
            }

            _context.Modules.AddRange(duplicatedModules);
            await _context.SaveChangesAsync();

            return duplicatedModules;
        }

        public async Task<Dictionary<int, (int LectureCount, int FlashCardCount, int AssessmentCount)>> GetContentCountsAsync(List<int> moduleIds)
        {
            var contentCounts = await _context.Modules
                .AsNoTracking()
                .Where(m => moduleIds.Contains(m.ModuleId))
                .Select(m => new
                {
                    m.ModuleId,
                    LectureCount = m.Lectures.Count,
                    FlashCardCount = m.FlashCards.Count,
                    AssessmentCount = m.Assessments.Count
                })
                .ToDictionaryAsync(
                    x => x.ModuleId,
                    x => (x.LectureCount, x.FlashCardCount, x.AssessmentCount)
                );

            return contentCounts;
        }

        public async Task<bool> BelongsToLessonAsync(int moduleId, int lessonId)
        {
            return await _context.Modules
                .AnyAsync(m => m.ModuleId == moduleId && m.LessonId == lessonId);
        }

        public async Task<bool> CanUserAccessModuleAsync(int moduleId, int userId)
        {
            return await _context.Modules
                .AsNoTracking()
                .Include(m => m.Lesson)
                .ThenInclude(l => l!.Course)
                .ThenInclude(c => c!.UserCourses)
                .AnyAsync(m => m.ModuleId == moduleId && 
                             (m.Lesson!.Course!.TeacherId == userId || 
                              m.Lesson.Course.UserCourses.Any(uc => uc.UserId == userId)));
        }
    }
}
