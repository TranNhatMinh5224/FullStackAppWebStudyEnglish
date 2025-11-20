using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace LearningEnglish.Infrastructure.Repositories
{
    public class TeacherSubscriptionRepository : ITeacherSubscriptionRepository
    {
        private readonly AppDbContext _context;

        public TeacherSubscriptionRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddTeacherSubscriptionAsync(TeacherSubscription teacherSubscription)
        {
            _context.TeacherSubscriptions.Add(teacherSubscription);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteTeacherSubscriptionAsync(TeacherSubscription IdSubcription)
        {
            _context.TeacherSubscriptions.Remove(IdSubcription);
            await _context.SaveChangesAsync();
        }




    }
}
