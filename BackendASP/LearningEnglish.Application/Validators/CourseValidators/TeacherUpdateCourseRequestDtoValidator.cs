using LearningEnglish.Application.DTOs;
using FluentValidation;

namespace LearningEnglish.Application.Validators.CourseValidators
{
    public class TeacherUpdateCourseRequestDtoValidator : AbstractValidator<TeacherUpdateCourseRequestDto>
    {
        public TeacherUpdateCourseRequestDtoValidator()
        {
            // Title là optional, nhưng nếu có thì phải valid
            RuleFor(x => x.Title)
                .MaximumLength(200).WithMessage("Course title must not exceed 200 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Title));

            // Description là optional, nhưng nếu có thì phải valid
            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Course description must not exceed 2000 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            // MaxStudent là optional, nhưng nếu có thì phải > 0
            RuleFor(x => x.MaxStudent)
                .GreaterThan(0).WithMessage("MaxStudent must be greater than 0")
                .When(x => x.MaxStudent.HasValue);
        }
    }
}
