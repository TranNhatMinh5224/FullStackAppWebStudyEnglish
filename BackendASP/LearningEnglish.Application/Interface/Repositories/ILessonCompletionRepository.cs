using LearningEnglish.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
    public interface ILessonCompletionRepository
    {
        // Lấy tiến độ lesson của user
        // RLS: User chỉ xem completions của chính mình, Teacher xem completions của students trong own courses, Admin xem tất cả
        // userId parameter: Defense in depth (RLS + userId filter) + Teacher có thể query completions của students
        Task<LessonCompletion?> GetByUserAndLessonAsync(int userId, int lessonId);
        
        // Lấy tất cả tiến độ của user
        // RLS: User chỉ xem completions của chính mình, Admin xem tất cả (có permission)
        // userId parameter: Defense in depth (RLS + userId filter) + Admin có thể query completions của user khác
        Task<List<LessonCompletion>> GetByUserIdAsync(int userId);
        
        // Lấy tiến độ nhiều lesson
        // RLS: User chỉ xem completions của chính mình, Admin xem tất cả (có permission)
        // userId parameter: Defense in depth (RLS + userId filter)
        Task<List<LessonCompletion>> GetByUserAndLessonIdsAsync(int userId, List<int> lessonIds);
        
        // Đếm số lesson đã hoàn thành
        // RLS: User chỉ đếm completions của chính mình, Admin đếm tất cả (có permission)
        // userId parameter: Defense in depth (RLS + userId filter)
        Task<int> CountCompletedLessonsByUserAsync(int userId);
        
        // Thêm tiến độ lesson
        Task AddAsync(LessonCompletion lessonCompletion);
        
        // Cập nhật tiến độ lesson
        Task UpdateAsync(LessonCompletion lessonCompletion);
    }
}
