using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface ILessonRepository
    {

        Task<List<Lesson>> GetListLessonByCourseId(int CourseId);
        Task<Lesson?> GetLessonById(int lessonId);
        Task AddLesson(Lesson lesson);
        Task UpdateLesson(Lesson lesson);
        Task DeleteLesson(int lessonId);

        Task<bool> LessonIncourse(string newtitle, int courseId);
        // đếm số lesson trong course 
        Task<int> CountLessonInCourse(int courseId);


    }
}
