using LearningEnglish.Application.DTOs;
using FluentValidation;

namespace LearningEnglish.Application.Validators.CourseValidators
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
