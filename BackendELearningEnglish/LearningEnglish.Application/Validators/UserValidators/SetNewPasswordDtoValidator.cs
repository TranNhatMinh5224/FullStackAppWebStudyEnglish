using LearningEnglish.Application.DTOs;
using FluentValidation;

namespace LearningEnglish.Application.Validators.User
{
    public class SetNewPasswordDtoValidator : AbstractValidator<SetNewPasswordDto>
    {
        public SetNewPasswordDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Invalid email format");

            RuleFor(x => x.OtpCode)
                .NotEmpty()
                .WithMessage("OTP code is required")
                .Length(6)
                .WithMessage("OTP code must be exactly 6 digits")
                .Matches(@"^\d{6}$")
                .WithMessage("OTP code must contain only digits");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("New password is required")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$")
                .WithMessage("Password must contain at least 8 characters, including uppercase, lowercase, number and special character");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .WithMessage("Confirm password is required")
                .Equal(x => x.NewPassword)
                .WithMessage("Confirm password must match the new password");
        }
    }
}
