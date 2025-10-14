using CleanDemo.Application.DTOs;
using FluentValidation;

namespace CleanDemo.Application.Validators.User
{
    public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.OtpCode)
                .NotEmpty()
                .Length(6)
                .Matches(@"^\d{6}$")
                .WithMessage("OTP code must contain exactly 6 digits");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(6);
        }
    }
}
