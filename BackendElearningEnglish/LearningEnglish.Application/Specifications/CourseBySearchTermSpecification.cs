using LearningEnglish.Application.Common.Specifications;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Specifications
{
    // Specification để search courses theo keyword trong Title hoặc ClassCode
    public class CourseBySearchTermSpecification : BaseSpecification<Course>
    {
        public CourseBySearchTermSpecification(string searchTerm)
            : base(c => c.Title.Contains(searchTerm) || 
                       (c.ClassCode != null && c.ClassCode.Contains(searchTerm)))
        {
        }
    }
}
