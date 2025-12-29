using FluentValidation;
using LearningEnglish.Application.DTOs.Admin;

namespace LearningEnglish.Application.Validators.AdminValidators
{
    public class UpgradeUserToTeacherDtoValidator : AbstractValidator<UpgradeUserToTeacherDto>
    {
        public UpgradeUserToTeacherDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống")
                .EmailAddress().WithMessage("Email không hợp lệ");

            RuleFor(x => x.TeacherPackageId)
                .GreaterThan(0).WithMessage("TeacherPackageId phải lớn hơn 0");
        }
    }
}

