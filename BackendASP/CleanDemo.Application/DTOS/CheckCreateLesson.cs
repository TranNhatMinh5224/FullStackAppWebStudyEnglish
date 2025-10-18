using CleanDemo.Domain.Enums;

namespace CleanDemo.Application.DTOs
{
    public class ReqCheckCreateLesson
    {
        public int CourseId { get; set; }


    }

    public class ResCheckCreateLesson
    {

        public bool CanCreateLesson { get; set; }
        public int QuanlityLessonFuture { get; set; }
    }
}