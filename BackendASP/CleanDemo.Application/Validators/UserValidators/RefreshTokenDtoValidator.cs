using CleanDemo.Application.DTOs;
using FluentValidation;

namespace CleanDemo.Application.Validators.User
{
    public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
    {
        public RefreshTokenDtoValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty();
        }
    }
}
