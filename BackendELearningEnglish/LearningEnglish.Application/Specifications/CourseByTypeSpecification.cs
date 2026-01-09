using LearningEnglish.Application.Common.Specifications;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Specifications
{
    // Specification để filter courses theo Type
    // Tuân thủ Open-Closed Principle: thêm specification mới thay vì sửa repository
    public class CourseByTypeSpecification : BaseSpecification<Course>
    {
        public CourseByTypeSpecification(CourseType type)
            : base(c => c.Type == type)
        {
        }
    }
}
