using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface ITeacherSubscriptionRepository
    {
        Task AddTeacherSubscriptionAsync(TeacherSubscription teacherSubscription);
        Task DeleteTeacherSubscriptionAsync(TeacherSubscription IdSubcription);
        Task<TeacherSubscription?> GetActiveSubscriptionAsync(int userId);
    }
}
