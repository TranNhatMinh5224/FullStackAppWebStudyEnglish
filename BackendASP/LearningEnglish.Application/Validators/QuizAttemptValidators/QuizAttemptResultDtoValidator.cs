using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.QuizAttemptValidators;

public class QuizAttemptResultDtoValidator : AbstractValidator<QuizAttemptResultDto>
{
    public QuizAttemptResultDtoValidator()
    {
        RuleFor(x => x.AttemptId)
            .GreaterThan(0)
            .WithMessage("AttemptId phải lớn hơn 0");

        RuleFor(x => x.TotalScore)
            .GreaterThanOrEqualTo(0)
            .WithMessage("TotalScore không được âm");

        RuleFor(x => x.Percentage)
            .InclusiveBetween(0, 100)
            .WithMessage("Percentage phải từ 0 đến 100");

        RuleFor(x => x.TimeSpentSeconds)
            .GreaterThanOrEqualTo(0)
            .WithMessage("TimeSpentSeconds không được âm");

        RuleFor(x => x.ScoresByQuestion)
            .NotNull()
            .WithMessage("ScoresByQuestion không được null");

        RuleFor(x => x.CorrectAnswers)
            .NotNull()
            .WithMessage("CorrectAnswers không được null");
    }
}
