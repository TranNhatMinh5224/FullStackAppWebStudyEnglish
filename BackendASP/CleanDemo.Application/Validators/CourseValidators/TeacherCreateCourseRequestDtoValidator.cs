using CleanDemo.Application.DTOs;
using CleanDemo.Domain.Enums;
using FluentValidation;

namespace CleanDemo.Application.Validators.CourseValidators
{
    public class TeacherCreateCourseRequestDtoValidator : AbstractValidator<TeacherCreateCourseRequestDto>
    {
        public TeacherCreateCourseRequestDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Course title is required")
                .MaximumLength(200).WithMessage("Course title must not exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Course description is required")
                .MaximumLength(2000).WithMessage("Course description must not exceed 2000 characters");

            RuleFor(x => x.Img)
                .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Img));

            // Teacher courses không có price (miễn phí)
            RuleFor(x => x.Type)
                .Equal(CourseType.Teacher).WithMessage("Teacher can only create Teacher type courses");
        }
    }
}
