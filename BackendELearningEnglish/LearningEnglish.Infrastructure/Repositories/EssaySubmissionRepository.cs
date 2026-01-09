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

            // Reload with User navigation property for mapping
            return await _context.EssaySubmissions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SubmissionId == submission.SubmissionId) ?? submission;
        }

        public async Task<EssaySubmission?> GetSubmissionByIdAsync(int submissionId)
        {
            return await _context.EssaySubmissions
                .Include(s => s.User)
                .Include(s => s.Essay)
                    .ThenInclude(e => e.Assessment)
                        .ThenInclude(a => a!.Module)
                            .ThenInclude(m => m!.Lesson)
                                .ThenInclude(l => l!.Course)
                .Include(s => s.GradedByTeacher)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);
        }

        public async Task<List<EssaySubmission>> GetSubmissionsByEssayIdPagedAsync(int essayId, int pageNumber, int pageSize)
        {
            return await _context.EssaySubmissions
                .Include(s => s.User)
                .Include(s => s.Essay)
                .Where(s => s.EssayId == essayId)
                .OrderByDescending(s => s.SubmittedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<EssaySubmission>> GetSubmissionsByEssayIdAsync(int essayId)
        {
            return await _context.EssaySubmissions
                .Include(s => s.User)
                .Include(s => s.Essay)
                .Where(s => s.EssayId == essayId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }

        public async Task<int> GetSubmissionsCountByEssayIdAsync(int essayId)
        {
            return await _context.EssaySubmissions
                .Where(s => s.EssayId == essayId)
                .CountAsync();
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

            // Update content
            existingSubmission.TextContent = submission.TextContent;
            existingSubmission.AttachmentKey = submission.AttachmentKey;
            existingSubmission.AttachmentType = submission.AttachmentType;
            existingSubmission.Status = submission.Status;

            // Update AI grading
            existingSubmission.Score = submission.Score;
            existingSubmission.Feedback = submission.Feedback;
            existingSubmission.GradedAt = submission.GradedAt;

            // Update Teacher grading
            existingSubmission.TeacherScore = submission.TeacherScore;
            existingSubmission.TeacherFeedback = submission.TeacherFeedback;
            existingSubmission.GradedByTeacherId = submission.GradedByTeacherId;
            existingSubmission.TeacherGradedAt = submission.TeacherGradedAt;

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
    }
}