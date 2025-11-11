using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class AssessmentRepository : IAssessmentRepository
    {
        private readonly AppDbContext _context;

        public AssessmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAssessment(Assessment assessment)
        {
            await _context.Assessments.AddAsync(assessment);
            await _context.SaveChangesAsync();
        }

        public async Task<Assessment?> GetAssessmentById(int assessmentId)
        {
            return await _context.Assessments.FindAsync(assessmentId);
        }
        public async Task<List<Assessment>> GetAssessmentsByModuleId(int moduleId)
        {
            return await _context.Assessments
                .Where(a => a.ModuleId == moduleId)
                .ToListAsync();
        }
        public async Task UpdateAssessment(Assessment assessment)
        {
            var existingAssessment = await _context.Assessments.FindAsync(assessment.AssessmentId);
            if (existingAssessment == null)
            {
                throw new ArgumentException("Assessment không tồn tại");
            }
            _context.Assessments.Update(assessment);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAssessment(int assessmentId)
        {
            var assessment = await _context.Assessments.FindAsync(assessmentId);
            if (assessment != null)
            {
                _context.Assessments.Remove(assessment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ModuleExists(int moduleId)
        {
            return await _context.Modules.AnyAsync(m => m.ModuleId == moduleId);
        }

        public async Task<bool> IsTeacherOwnerOfModule(int teacherId, int moduleId)
        {
            return await _context.Modules
                .Include(m => m.Lesson)
                    .ThenInclude(l => l!.Course)
                .AnyAsync(m => m.ModuleId == moduleId &&
                              m.Lesson!.Course!.TeacherId == teacherId);
        }
    }
}