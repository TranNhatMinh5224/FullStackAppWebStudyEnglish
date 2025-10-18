using CleanDemo.Domain.Entities;

namespace CleanDemo.Application.Interface
{
    public interface ITeacherSubscriptionRepository
    {

        Task AddTeacherSubscriptionAsync(TeacherSubscription teacherSubscription);
        Task DeleteTeacherSubscriptionAsync(TeacherSubscription IdSubcription);




    }
}