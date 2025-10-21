using CleanDemo.Application.DTOs;
using CleanDemo.Domain.Enums;
using FluentValidation;
using CleanDemo.Application.Interface;

namespace CleanDemo.Application.Validators.CourseValidators
{
    public class TeacherCreateCourseRequestDtoValidator : AbstractValidator<TeacherCreateCourseRequestDto>
    {
        private readonly ICourseRepository _courseRepository;

        public TeacherCreateCourseRequestDtoValidator(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Course title is required")
                .MaximumLength(200).WithMessage("Course title must not exceed 200 characters")
                .MustAsync(async (dto, title, cancellation) =>
                {
                    if (string.IsNullOrWhiteSpace(title)) return true;
                    // No direct teacherId in validator, so skip uniqueness here; service will handle per-teacher uniqueness.
                    return true;
                }).WithMessage("Course title validation failed");

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
