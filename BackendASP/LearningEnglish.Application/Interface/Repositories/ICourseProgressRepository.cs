using LearningEnglish.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
    public interface ICourseProgressRepository
    {
        // Lấy tiến độ khóa học của user
        Task<CourseProgress?> GetByUserAndCourseAsync(int userId, int courseId);
        
        // Lấy tất cả tiến độ của user
        Task<List<CourseProgress>> GetByUserIdAsync(int userId);
        
        // Đếm số khóa học đã hoàn thành
        Task<int> CountCompletedCoursesByUserAsync(int userId);
        
        // Thêm tiến độ khóa học
        Task AddAsync(CourseProgress courseProgress);
        
        // Cập nhật tiến độ khóa học
        Task UpdateAsync(CourseProgress courseProgress);
    }
}
