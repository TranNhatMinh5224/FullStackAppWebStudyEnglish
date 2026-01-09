using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.QuizAttemptValidators;

public class QuizAttemptDtoValidator : AbstractValidator<QuizAttemptDto>
{
    public QuizAttemptDtoValidator()
    {
        RuleFor(x => x.QuizId)
            .GreaterThan(0)
            .WithMessage("QuizId phải lớn hơn 0");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId phải lớn hơn 0");

        RuleFor(x => x.AttemptNumber)
            .GreaterThan(0)
            .WithMessage("AttemptNumber phải lớn hơn 0");

        RuleFor(x => x.TotalScore)
            .GreaterThanOrEqualTo(0)
            .WithMessage("TotalScore không được âm");

        RuleFor(x => x.TimeSpentSeconds)
            .GreaterThanOrEqualTo(0)
            .WithMessage("TimeSpentSeconds không được âm");
    }
}
