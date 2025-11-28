using LearningEnglish.Application.DTOs;
using FluentValidation;

namespace LearningEnglish.Application.Validators.CourseValidators
{
    public class AdminUpdateCourseRequestDtoValidator : AbstractValidator<AdminUpdateCourseRequestDto>
    {
        public AdminUpdateCourseRequestDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Course title is required")
                .MaximumLength(200).WithMessage("Course title must not exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Course description is required")
                .MaximumLength(2000).WithMessage("Course description must not exceed 2000 characters");

            RuleFor(x => x.ImageTempKey)
                .MaximumLength(500).WithMessage("Image temp key must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.ImageTempKey));

            RuleFor(x => x.ImageType)
                .MaximumLength(50).WithMessage("Image type must not exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.ImageType));

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
