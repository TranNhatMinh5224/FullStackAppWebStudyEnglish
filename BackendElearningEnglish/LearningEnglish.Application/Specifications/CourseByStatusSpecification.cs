using LearningEnglish.Application.Common.Specifications;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Specifications
{
    // Specification để filter courses theo Status
    public class CourseByStatusSpecification : BaseSpecification<Course>
    {
        public CourseByStatusSpecification(CourseStatus status)
            : base(c => c.Status == status)
        {
        }
    }
}
