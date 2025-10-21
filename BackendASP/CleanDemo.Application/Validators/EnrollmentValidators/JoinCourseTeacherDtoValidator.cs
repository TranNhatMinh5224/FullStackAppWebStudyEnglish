using CleanDemo.Application.DTOs;
using FluentValidation;

namespace CleanDemo.Application.Validators.EnrollmentValidators
{
    public class JoinCourseTeacherDtoValidator : AbstractValidator<JoinCourseTeacherDto>
    {
        public JoinCourseTeacherDtoValidator()
        {
            RuleFor(x => x.CourseId)
                .GreaterThan(0).WithMessage("Course ID must be greater than 0");
        }
    }
}
