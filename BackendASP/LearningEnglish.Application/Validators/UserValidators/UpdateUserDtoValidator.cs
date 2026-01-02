using LearningEnglish.Application.DTOs;
using FluentValidation;

namespace LearningEnglish.Application.Validators.User
{
    public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

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
