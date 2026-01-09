using LearningEnglish.Application.DTOs;
using FluentValidation;

namespace LearningEnglish.Application.Validators.TeacherPackageValidators
{
    public class UpdateTeacherPackageDtoValidator : AbstractValidator<UpdateTeacherPackageDto>
    {
        public UpdateTeacherPackageDtoValidator()
        {
            // Chỉ validate khi có giá trị (nullable - hỗ trợ partial update)
            RuleFor(x => x.PackageName)
                .MaximumLength(100).WithMessage("Package name must not exceed 100 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.PackageName));

            RuleFor(x => x.Level)
                .IsInEnum().WithMessage("Invalid package level")
                .When(x => x.Level.HasValue);

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0")
                .LessThanOrEqualTo(100000000).WithMessage("Price must not exceed 100,000,000")
                .When(x => x.Price.HasValue);

            RuleFor(x => x.MaxCourses)
                .GreaterThan(0).WithMessage("Max courses must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Max courses must not exceed 100")
                .When(x => x.MaxCourses.HasValue);

            RuleFor(x => x.MaxLessons)
                .GreaterThan(0).WithMessage("Max lessons must be greater than 0")
                .LessThanOrEqualTo(1000).WithMessage("Max lessons must not exceed 1000")
                .When(x => x.MaxLessons.HasValue);

            RuleFor(x => x.MaxStudents)
                .GreaterThan(0).WithMessage("Max students must be greater than 0")
                .LessThanOrEqualTo(10000).WithMessage("Max students must not exceed 10,000")
                .When(x => x.MaxStudents.HasValue);
        }
    }
}
