using LearningEnglish.Application.DTOs;
using FluentValidation;

namespace LearningEnglish.Application.Validators.CourseValidators
{
    public class AdminCreateCourseRequestDtoValidator : AbstractValidator<AdminCreateCourseRequestDto>
    {
        public AdminCreateCourseRequestDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Course title is required")
                .MaximumLength(200).WithMessage("Course title must not exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Course description is required")
                .MaximumLength(2000).WithMessage("Course description must not exceed 2000 characters");


            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0")
                .When(x => x.Price.HasValue);

            RuleFor(x => x.MaxStudent)
                .GreaterThanOrEqualTo(0).WithMessage("MaxStudent must be greater than or equal to 0 (0 means unlimited)");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid course type");
        }
    }
}
