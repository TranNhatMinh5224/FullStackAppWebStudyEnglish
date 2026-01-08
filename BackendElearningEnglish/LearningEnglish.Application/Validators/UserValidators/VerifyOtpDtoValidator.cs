using LearningEnglish.Application.DTOs;
using FluentValidation;

namespace LearningEnglish.Application.Validators.User
{
    public class VerifyOtpDtoValidator : AbstractValidator<VerifyOtpDto>
    {
        public VerifyOtpDtoValidator()
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
        }
    }
}
