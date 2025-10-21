using CleanDemo.Application.DTOs;
using FluentValidation;
using CleanDemo.Application.Interface;

namespace CleanDemo.Application.Validators.CourseValidators
{
    public class AdminCreateCourseRequestDtoValidator : AbstractValidator<AdminCreateCourseRequestDto>
    {
        private readonly ICourseRepository _courseRepository;

        public AdminCreateCourseRequestDtoValidator(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Course title is required")
                .MaximumLength(200).WithMessage("Course title must not exceed 200 characters")
                .MustAsync(async (title, cancellation) =>
                {
                    if (string.IsNullOrWhiteSpace(title)) return true; // other rule will catch emptiness
                    var existing = await _courseRepository.GetSystemCourses();
                    return !existing.Any(c => string.Equals(c.Title?.Trim(), title.Trim(), StringComparison.OrdinalIgnoreCase));
                }).WithMessage("A system course with this title already exists");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Course description is required")
                .MaximumLength(2000).WithMessage("Course description must not exceed 2000 characters");

            RuleFor(x => x.Img)
                .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Img));

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0")
                .When(x => x.Price.HasValue);

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid course type");
        }
    }
}
