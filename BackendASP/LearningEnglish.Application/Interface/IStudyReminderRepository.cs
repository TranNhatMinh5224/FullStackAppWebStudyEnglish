using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
    public interface IStudyReminderRepository
    {
        Task<StudyReminder?> GetByIdAsync(int id);
        Task<IEnumerable<StudyReminder>> GetUserStudyRemindersAsync(int userId);
        Task<IEnumerable<StudyReminder>> GetActiveRemindersForTimeAsync(string time, DaysOfWeek dayOfWeek);
        Task AddAsync(StudyReminder reminder);
        Task UpdateAsync(StudyReminder reminder);
        Task DeleteAsync(int id);
    }
}
