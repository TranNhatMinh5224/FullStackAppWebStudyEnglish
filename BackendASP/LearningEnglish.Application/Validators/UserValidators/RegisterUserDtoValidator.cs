using LearningEnglish.Application.DTOs;
using FluentValidation;

namespace LearningEnglish.Application.Validators.User
{
    public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
    {
        public RegisterUserDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters");

            // Phone number: Optional (có thể để trống), nhưng nếu nhập thì phải đúng format
            RuleFor(x => x.PhoneNumber)
                .Matches(@"^0\d{9}$").WithMessage("Phone number must be 10 digits starting with 0")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber)); // Chỉ validate khi có giá trị

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.UtcNow).WithMessage("Date of birth must be in the past")
                .Must(dob => dob == null || dob.Value.Year >= 1900).WithMessage("Date of birth must be after 1900")
                .When(x => x.DateOfBirth.HasValue);
        }
    }
}
