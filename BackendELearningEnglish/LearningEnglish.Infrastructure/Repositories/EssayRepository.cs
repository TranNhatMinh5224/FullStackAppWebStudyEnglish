using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class EssayRepository : IEssayRepository
    {
        private readonly AppDbContext _context;

        public EssayRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task<Essay> CreateEssayAsync(Essay essay)
        {
            _context.Essays.Add(essay);
            await _context.SaveChangesAsync();
            return essay;
        }

        public async Task<Essay?> GetEssayByIdAsync(int essayId)
        {
            return await _context.Essays
                .Include(e => e.Assessment)
                    .ThenInclude(a => a!.Module)
                        .ThenInclude(m => m!.Lesson)
                            .ThenInclude(l => l!.Course)
                .FirstOrDefaultAsync(e => e.EssayId == essayId);
        }

        public async Task<Essay?> GetEssayByIdWithDetailsAsync(int essayId)
        {
            return await _context.Essays
                .Include(e => e.Assessment)
                    .ThenInclude(a => a!.Module)
                .Include(e => e.EssaySubmissions)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(e => e.EssayId == essayId);
        }

        public async Task<List<Essay>> GetEssaysByAssessmentIdAsync(int assessmentId)
        {
            return await _context.Essays
                .Include(e => e.Assessment)
                .Where(e => e.AssessmentId == assessmentId)
                .ToListAsync();
        }

        public async Task<Essay> UpdateEssayAsync(Essay essay)
        {
            var existingEssay = await _context.Essays.FindAsync(essay.EssayId);
            if (existingEssay == null)
            {
                throw new ArgumentException("Essay không tồn tại");
            }

            existingEssay.Title = essay.Title;
            existingEssay.Description = essay.Description;

            await _context.SaveChangesAsync();
            return existingEssay;
        }

        public async Task DeleteEssayAsync(int essayId)
        {
            var essay = await _context.Essays.FindAsync(essayId);
            if (essay != null)
            {
                _context.Essays.Remove(essay);
                await _context.SaveChangesAsync();
            }
        }



        public async Task<bool> AssessmentExistsAsync(int assessmentId)
        {
            return await _context.Assessments.AnyAsync(a => a.AssessmentId == assessmentId);
        }

        public async Task<bool> IsTeacherOwnerOfAssessmentAsync(int teacherId, int assessmentId)
        {
            return await _context.Assessments
                .Include(a => a.Module)
                    .ThenInclude(m => m!.Lesson)
                        .ThenInclude(l => l!.Course)
                .AnyAsync(a => a.AssessmentId == assessmentId &&
                              a.Module!.Lesson!.Course!.TeacherId == teacherId);
        }

        public async Task<bool> EssayExistsAsync(int essayId)
        {
            return await _context.Essays.AnyAsync(e => e.EssayId == essayId);
        }
    }
}