using CleanDemo.Application.DTOs;
using FluentValidation;

namespace CleanDemo.Application.Validators.User
{
    public class UpdateUserRoleDtoValidator : AbstractValidator<UpdateUserRoleDto>
    {
        public UpdateUserRoleDtoValidator()
        {
            RuleFor(x => x.RoleName)
                .NotEmpty().WithMessage("Role name is required");
        }
    }
}
