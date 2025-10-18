using CleanDemo.Application.DTOs;
using FluentValidation;

namespace CleanDemo.Application.Validators.LessonValidators
{
    public class TeacherCreateLessonDtoValidator : AbstractValidator<TeacherCreateLessonDto>
    {
        public TeacherCreateLessonDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Lesson title is required")
                .MaximumLength(200).WithMessage("Lesson title must not exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Lesson description is required")
                .MaximumLength(2000).WithMessage("Lesson description must not exceed 2000 characters");

            RuleFor(x => x.CourseId)
                .GreaterThan(0).WithMessage("Course ID must be greater than 0");
        }
    }
}
