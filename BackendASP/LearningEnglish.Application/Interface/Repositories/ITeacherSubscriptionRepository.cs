using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface ITeacherSubscriptionRepository
    {
        // Thêm đăng ký giáo viên
        // RLS: Teacher chỉ tạo subscriptions cho chính mình, Admin có thể tạo cho bất kỳ teacher nào
        Task AddTeacherSubscriptionAsync(TeacherSubscription teacherSubscription);
        
        // Xóa đăng ký giáo viên
        // RLS: Teacher chỉ xóa subscriptions của chính mình, Admin xóa được tất cả
        Task DeleteTeacherSubscriptionAsync(TeacherSubscription IdSubcription);
        
        // Lấy đăng ký đang active
        // RLS: Teacher chỉ xem subscriptions của chính mình, Admin xem tất cả (có permission)
        // userId parameter: Defense in depth (RLS + userId filter) + Admin có thể query subscriptions của teacher khác
        Task<TeacherSubscription?> GetActiveSubscriptionAsync(int userId);
    }
}
