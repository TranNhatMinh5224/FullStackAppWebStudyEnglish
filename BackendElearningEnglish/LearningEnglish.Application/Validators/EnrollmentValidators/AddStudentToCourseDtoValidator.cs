using LearningEnglish.Application.DTOs;
using FluentValidation;

namespace LearningEnglish.Application.Validators.EnrollmentValidators
{
    public class AddStudentToCourseDtoValidator : AbstractValidator<AddStudentToCourseDto>
    {
        public AddStudentToCourseDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(255).WithMessage("Email must not exceed 255 characters");
        }
    }
}

