using CleanDemo.Domain.Entities;

namespace CleanDemo.Application.Interface
{
    public interface ILessonRepository
    {
        // CRUD cơ bản 
        Task<Lesson> GetListLessonByCourseId(int CourseId);
        Task<Lesson?> GetByIdAsync(int lessonId);
        Task AddLesson(Lesson lesson);
        Task UpdateLesson(Lesson lesson);
        Task DeleteLesson(int lessonId);


    }
}