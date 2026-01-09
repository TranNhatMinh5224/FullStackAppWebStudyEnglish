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

        // Admin: Lấy tất cả subscriptions
        Task<List<TeacherSubscription>> GetAllTeacherSubscriptionsAsync();

        // Admin: Lấy subscription theo ID
        Task<TeacherSubscription?> GetTeacherSubscriptionByIdAsync(int subscriptionId);

        // Lấy subscription theo ID và UserId (để check ownership)
        Task<TeacherSubscription?> GetTeacherSubscriptionByIdAndUserIdAsync(int subscriptionId, int userId);

        // Admin: Lấy subscriptions của user
        Task<List<TeacherSubscription>> GetTeacherSubscriptionsByUserIdAsync(int userId);
    }
}
