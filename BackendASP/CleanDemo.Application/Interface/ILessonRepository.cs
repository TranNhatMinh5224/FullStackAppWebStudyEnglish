using CleanDemo.Domain.Entities;

namespace CleanDemo.Application.Interface
{
    public interface ILessonRepository
    {

        Task<List<Lesson>> GetListLessonByCourseId(int CourseId);
        Task<Lesson?> GetLessonById(int lessonId);
        Task AddLesson(Lesson lesson);
        Task UpdateLesson(Lesson lesson);
        Task DeleteLesson(int lessonId);
        // Kiểm tra sự tồn tại của lesson trong Course chưa
        Task<bool> LessonIncourse(string newtitle, int courseId);


    }
}