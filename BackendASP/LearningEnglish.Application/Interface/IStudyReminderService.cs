using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
    public interface IStudyReminderService
    {
        Task<ServiceResponse<StudyReminderDto>> CreateStudyReminderAsync(CreateStudyReminderDto request);

        Task<ServiceResponse<List<StudyReminderDto>>> GetUserStudyRemindersAsync(int userId);

        Task<ServiceResponse<StudyReminderDto>> UpdateStudyReminderAsync(int reminderId, CreateStudyReminderDto request, int userId);

        Task<ServiceResponse<bool>> DeleteStudyReminderAsync(int reminderId, int userId);

        Task<ServiceResponse<bool>> ToggleStudyReminderAsync(int reminderId, int userId);

        Task<ServiceResponse<bool>> SendReminderNowAsync(int reminderId, int userId);
    }
}
