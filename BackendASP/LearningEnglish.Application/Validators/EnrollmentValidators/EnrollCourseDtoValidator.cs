using LearningEnglish.Application.DTOs;
using FluentValidation;

namespace LearningEnglish.Application.Validators.EnrollmentValidators
{
    public class EnrollCourseDtoValidator : AbstractValidator<EnrollCourseDto>
    {
        public EnrollCourseDtoValidator()
        {
            RuleFor(x => x.CourseId)
                .GreaterThan(0).WithMessage("Course ID must be greater than 0");
        }
    }
}
