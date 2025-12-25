using LearningEnglish.Application.Common.Specifications;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Specifications
{
    // Specification để eager load related entities cho Course
    public class CourseWithIncludesSpecification : BaseSpecification<Course>
    {
        public CourseWithIncludesSpecification(bool includeTeacher = true, bool includeLessons = false)
        {
            if (includeTeacher)
            {
                AddInclude(c => c.Teacher!);
            }
            
            if (includeLessons)
            {
                AddInclude(c => c.Lessons);
            }
        }
    }
}
