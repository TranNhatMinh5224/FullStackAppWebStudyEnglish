using LearningEnglish.Application.Common.Specifications;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Specifications
{
    // Specification để filter courses theo teacherId
    public class CourseByTeacherSpecification : BaseSpecification<Course>
    {
        public CourseByTeacherSpecification(int teacherId)
            : base(c => c.TeacherId == teacherId)
        {
            AddInclude(c => c.Teacher!);
        }
    }
}
