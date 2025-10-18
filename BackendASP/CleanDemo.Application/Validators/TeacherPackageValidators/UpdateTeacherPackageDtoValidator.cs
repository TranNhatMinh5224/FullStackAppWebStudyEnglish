using CleanDemo.Application.DTOs;
using FluentValidation;

namespace CleanDemo.Application.Validators.TeacherPackageValidators
{
    public class UpdateTeacherPackageDtoValidator : AbstractValidator<UpdateTeacherPackageDto>
    {
        public UpdateTeacherPackageDtoValidator()
        {
            RuleFor(x => x.TeacherPackageId)
                .GreaterThan(0).WithMessage("Package ID must be greater than 0");

            RuleFor(x => x.PackageName)
                .NotEmpty().WithMessage("Package name is required")
                .MaximumLength(100).WithMessage("Package name must not exceed 100 characters");

            RuleFor(x => x.Level)
                .IsInEnum().WithMessage("Invalid package level");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0")
                .LessThanOrEqualTo(10000).WithMessage("Price must not exceed 10,000");

            RuleFor(x => x.MaxCourses)
                .GreaterThan(0).WithMessage("Max courses must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Max courses must not exceed 100");

            RuleFor(x => x.MaxLessons)
                .GreaterThan(0).WithMessage("Max lessons must be greater than 0")
                .LessThanOrEqualTo(1000).WithMessage("Max lessons must not exceed 1000");

            RuleFor(x => x.MaxStudents)
                .GreaterThan(0).WithMessage("Max students must be greater than 0")
                .LessThanOrEqualTo(10000).WithMessage("Max students must not exceed 10,000");
        }
    }
}
