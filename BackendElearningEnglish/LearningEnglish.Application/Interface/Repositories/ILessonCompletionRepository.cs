using LearningEnglish.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
    public interface ILessonCompletionRepository
    {
        // Lấy tiến độ lesson của user
        Task<LessonCompletion?> GetByUserAndLessonAsync(int userId, int lessonId);
        
        // Lấy tất cả tiến độ của user
        Task<List<LessonCompletion>> GetByUserIdAsync(int userId);
        
        // Lấy tiến độ nhiều lesson
        Task<List<LessonCompletion>> GetByUserAndLessonIdsAsync(int userId, List<int> lessonIds);
        
        // Đếm số lesson đã hoàn thành
        Task<int> CountCompletedLessonsByUserAsync(int userId);
        
        // Thêm tiến độ lesson
        Task AddAsync(LessonCompletion lessonCompletion);
        
        // Cập nhật tiến độ lesson
        Task UpdateAsync(LessonCompletion lessonCompletion);
    }
}
