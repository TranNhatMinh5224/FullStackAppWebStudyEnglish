using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class EssaySubmissionRepository : IEssaySubmissionRepository
    {
        private readonly AppDbContext _context;

        public EssaySubmissionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<EssaySubmission> CreateSubmissionAsync(EssaySubmission submission)
        {
            submission.SubmittedAt = DateTime.UtcNow;
            submission.Status = SubmissionStatus.Submitted;

            _context.EssaySubmissions.Add(submission);
            await _context.SaveChangesAsync();
            return submission;
        }

        public async Task<EssaySubmission?> GetSubmissionByIdAsync(int submissionId)
        {
            return await _context.EssaySubmissions
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);
        }

        public async Task<EssaySubmission?> GetSubmissionByIdWithDetailsAsync(int submissionId)
        {
            return await _context.EssaySubmissions
                .Include(s => s.User)
                .Include(s => s.Essay)
                    .ThenInclude(e => e.Assessment)
                        .ThenInclude(a => a.Module)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);
        }

        public async Task<List<EssaySubmission>> GetSubmissionsByEssayIdAsync(int essayId)
        {
            return await _context.EssaySubmissions
                .Include(s => s.User)
                .Where(s => s.EssayId == essayId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }

        public async Task<List<EssaySubmission>> GetSubmissionsByUserIdAsync(int userId)
        {
            return await _context.EssaySubmissions
                .Include(s => s.Essay)
                    .ThenInclude(e => e.Assessment)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }

        public async Task<List<EssaySubmission>> GetSubmissionsByAssessmentIdAsync(int assessmentId)
        {
            return await _context.EssaySubmissions
                .Include(s => s.User)
                .Include(s => s.Essay)
                .Where(s => s.Essay.AssessmentId == assessmentId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }

        public async Task<EssaySubmission?> GetUserSubmissionForEssayAsync(int userId, int essayId)
        {
            return await _context.EssaySubmissions
                .Include(s => s.Essay)
                    .ThenInclude(e => e.Assessment)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.EssayId == essayId);
        }

        public async Task<EssaySubmission> UpdateSubmissionAsync(EssaySubmission submission)
        {
            var existingSubmission = await _context.EssaySubmissions.FindAsync(submission.SubmissionId);
            if (existingSubmission == null)
            {
                throw new ArgumentException("Submission không tồn tại");
            }

            existingSubmission.TextContent = submission.TextContent;
            existingSubmission.Status = submission.Status;

            await _context.SaveChangesAsync();
            return existingSubmission;
        }

        public async Task DeleteSubmissionAsync(int submissionId)
        {
            var submission = await _context.EssaySubmissions.FindAsync(submissionId);
            if (submission != null)
            {
                _context.EssaySubmissions.Remove(submission);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsUserOwnerOfSubmissionAsync(int userId, int submissionId)
        {
            return await _context.EssaySubmissions
                .AnyAsync(s => s.SubmissionId == submissionId && s.UserId == userId);
        }

        public async Task<bool> AssessmentExistsAsync(int assessmentId)
        {
            return await _context.Assessments.AnyAsync(a => a.AssessmentId == assessmentId);
        }
    }
}