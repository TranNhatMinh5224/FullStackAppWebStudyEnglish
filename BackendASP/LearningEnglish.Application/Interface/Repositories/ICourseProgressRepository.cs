using LearningEnglish.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
    public interface ICourseProgressRepository
    {
        // Lấy tiến độ khóa học của user
        // RLS: User chỉ xem progress của chính mình, Teacher xem progress của students trong own courses, Admin xem tất cả
        // userId parameter: CẦN THIẾT - Teacher/Admin query progress của student khác (GetStudentDetailInCourseAsync)
        // Defense in depth: RLS filter + userId filter
        Task<CourseProgress?> GetByUserAndCourseAsync(int userId, int courseId);
        
        // Lấy tất cả tiến độ của user
        // RLS: User chỉ xem progress của chính mình, Admin xem tất cả (có permission)
        // userId parameter: Defense in depth (RLS + userId filter) + Admin có thể query progress của user khác
        Task<List<CourseProgress>> GetByUserIdAsync(int userId);
        
        // Đếm số khóa học đã hoàn thành
        // RLS: User chỉ đếm progress của chính mình, Admin đếm tất cả (có permission)
        // userId parameter: Defense in depth (RLS + userId filter)
        Task<int> CountCompletedCoursesByUserAsync(int userId);
        
        // Thêm tiến độ khóa học
        Task AddAsync(CourseProgress courseProgress);
        
        // Cập nhật tiến độ khóa học
        Task UpdateAsync(CourseProgress courseProgress);
    }
}
