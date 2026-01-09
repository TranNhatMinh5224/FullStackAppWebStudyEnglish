using LearningEnglish.Application.DTOs;
using FluentValidation;

namespace LearningEnglish.Application.Validators.EnrollmentValidators
{
    public class EnrollCourseByClassCodeDtoValidator : AbstractValidator<EnrollCourseByClassCodeDto>
    {
        public EnrollCourseByClassCodeDtoValidator()
        {
            RuleFor(x => x.ClassCode)
                .NotEmpty().WithMessage("Class code is required")
                .Length(6, 20).WithMessage("Class code must be between 6 and 20 characters");
        }
    }
}

