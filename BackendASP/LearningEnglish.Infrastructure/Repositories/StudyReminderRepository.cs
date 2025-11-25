using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class StudyReminderRepository : IStudyReminderRepository
    {
        private readonly AppDbContext _context;

        public StudyReminderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<StudyReminder> GetByIdAsync(int id)
        {
            return await _context.StudyReminders.FindAsync(id);
        }

        public async Task<IEnumerable<StudyReminder>> GetUserStudyRemindersAsync(int userId)
        {
            return await _context.StudyReminders
                .Where(sr => sr.UserId == userId)
                .OrderBy(sr => sr.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<StudyReminder>> GetActiveRemindersForTimeAsync(string time, DaysOfWeek dayOfWeek)
        {
            return await _context.StudyReminders
                .Where(sr => sr.IsActive &&
                            sr.ScheduledTime == time &&
                            (sr.DaysOfWeek & dayOfWeek) != 0)
                .Include(sr => sr.User)
                .ToListAsync();
        }

        public async Task AddAsync(StudyReminder reminder)
        {
            await _context.StudyReminders.AddAsync(reminder);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(StudyReminder reminder)
        {
            _context.StudyReminders.Update(reminder);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var reminder = await GetByIdAsync(id);
            if (reminder != null)
            {
                _context.StudyReminders.Remove(reminder);
                await _context.SaveChangesAsync();
            }
        }
    }
}