using CleanDemo.Application.DTOs;
using FluentValidation;

namespace CleanDemo.Application.Validators.TeacherSubscriptionValidators
{
    public class PurchaseTeacherPackageDtoValidator : AbstractValidator<PurchaseTeacherPackageDto>
    {
        public PurchaseTeacherPackageDtoValidator()
        {
            RuleFor(x => x.IdTeacherPackage)
                .GreaterThan(0).WithMessage("Teacher package ID must be greater than 0");
        }
    }
}
