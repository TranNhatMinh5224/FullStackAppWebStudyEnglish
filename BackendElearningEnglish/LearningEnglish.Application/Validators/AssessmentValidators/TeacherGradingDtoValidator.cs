using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.AssessmentValidators;

public class TeacherGradingDtoValidator : AbstractValidator<TeacherGradingDto>
{
    public TeacherGradingDtoValidator()
    {
        RuleFor(x => x.Score)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Score must be greater than or equal to 0");

        RuleFor(x => x.Feedback)
            .MaximumLength(5000)
            .WithMessage("Feedback must not exceed 5000 characters");
    }
}