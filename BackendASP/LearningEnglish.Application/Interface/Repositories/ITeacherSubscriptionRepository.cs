using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface ITeacherSubscriptionRepository
    {
        // Thêm đăng ký giáo viên
        Task AddTeacherSubscriptionAsync(TeacherSubscription teacherSubscription);
        
        // Xóa đăng ký giáo viên
        Task DeleteTeacherSubscriptionAsync(TeacherSubscription IdSubcription);
        
        // Lấy đăng ký đang active
        Task<TeacherSubscription?> GetActiveSubscriptionAsync(int userId);
    }
}
