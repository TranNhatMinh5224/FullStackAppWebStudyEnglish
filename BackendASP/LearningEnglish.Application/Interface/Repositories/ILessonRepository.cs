using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface ILessonRepository
    {
        // Lấy danh sách lesson theo course
        Task<List<Lesson>> GetListLessonByCourseId(int CourseId);
        
        // Lấy lesson theo ID
        Task<Lesson?> GetLessonById(int lessonId);
        
        // Thêm lesson
        Task AddLesson(Lesson lesson);
        
        // Cập nhật lesson
        Task UpdateLesson(Lesson lesson);
        
        // Xóa lesson
        Task DeleteLesson(int lessonId);

        // Kiểm tra lesson có trong course
        Task<bool> LessonIncourse(string newtitle, int courseId);
        
        // Đếm số lesson trong course
        Task<int> CountLessonInCourse(int courseId);
        
        // Lấy course ID từ lesson ID
        Task<int?> GetCourseIdByLessonIdAsync(int lessonId);
    }
}
